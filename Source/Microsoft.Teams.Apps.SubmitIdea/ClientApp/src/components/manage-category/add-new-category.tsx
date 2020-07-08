// <copyright file="add-new-category.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Text, Flex, Input, TextArea, Button, ChevronStartIcon } from "@fluentui/react-northstar";
import { createBrowserHistory } from "history";
import * as microsoftTeams from "@microsoft/teams-js";
import { SeverityLevel, ApplicationInsights } from "@microsoft/applicationinsights-web";
import { getApplicationInsightsInstance } from "../../helpers/app-insights";
import { postCategory } from "../../api/category-api";
import { ICategoryDetails, CategoryDetails } from "../models/category";
import { withTranslation, WithTranslation } from "react-i18next";
import { isNullorWhiteSpace } from "../../helpers/helper";

const browserHistory = createBrowserHistory({ basename: "" });

interface ICategoryState {
    categoryName: string;
    categoryDescription: string;
    isNameValuePresent: boolean,
    isDescriptionValuePresent: boolean,
    error: string,
    isSubmitLoading: boolean,
}

interface ICategoryProps extends WithTranslation {
    categories: Array<ICategoryDetails>,
    isNewAllowed: boolean,
    teamId: string,
    onBackButtonClick: () => void,
    onSuccess: (operation: string) => void
}

class AddCategory extends React.Component<ICategoryProps, ICategoryState> {
    telemetry?: any = null;
    theme?: any = null;
    locale?: string | null;
    appInsights: any;
    userObjectId?: string = "";

    constructor(props: any) {
        super(props);

        this.state = {
            categoryName: "",
            categoryDescription: "",
            isNameValuePresent: true,
            isDescriptionValuePresent: true,
            error: "",
            isSubmitLoading: false,
        }

        let search = window.location.search;
        let params = new URLSearchParams(search);
        this.telemetry = params.get("telemetry");
        this.theme = params.get("theme");
        this.locale = params.get("locale");
    }

    /**
    * Used to initialize Microsoft Teams sdk
    */
    async componentDidMount() {

        microsoftTeams.initialize();
        microsoftTeams.getContext((context) => {
            this.userObjectId = context.userObjectId;

            // Initialize application insights for logging events and errors.
            this.appInsights = getApplicationInsightsInstance(this.telemetry, browserHistory);
        });
    }

    /**
   *Checks whether all validation conditions are matched before user submits new response
   */
    checkIfSubmitAllowed = (t: any) => {
        if (isNullorWhiteSpace(this.state.categoryName)) {
            this.setState({ isNameValuePresent: false });
            return false;
        }

        if (isNullorWhiteSpace(this.state.categoryDescription)) {
            this.setState({ isDescriptionValuePresent: false });
            return false;
        }

        if (this.state.categoryName && this.state.categoryDescription) {
            let filteredData = this.props.categories.filter((category) => {
                return (category.categoryName?.toUpperCase() === this.state.categoryName.trim().toUpperCase());
            });

            if (filteredData.length > 0) {
                this.setState({ error: t('duplicateCategoryError') })

                return false;
            }
            
            return true;
        }
        else {
            return false;
        }
    }

    /**
     * Handle add category event.
    */
    onAddButtonClick = async (t: any) => {
        if (this.checkIfSubmitAllowed(t)) {
            this.setState({ isSubmitLoading: true });
            let categoryDetail: CategoryDetails = {
                categoryId: undefined,
                categoryName: this.state.categoryName.trim(),
                categoryDescription: this.state.categoryDescription.trim(),
                createdByUserId: this.userObjectId,
                modifiedByUserId: undefined,
                createdOn: undefined
            };

            this.appInsights.trackTrace({ message: `'addCategory' - Initiated request`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });            
            let response = await postCategory(categoryDetail);

            if (response.status === 200 && response.data) {
                this.appInsights.trackTrace({ message: `'addCategory' - Request success`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
                this.appInsights.trackEvent({ name: `Add category` }, { User: this.userObjectId, Team: this.props.teamId });
                this.setState({ error: '', isSubmitLoading: false });
                this.props.onSuccess("add");
                return;
            }
            else {
                this.appInsights.trackTrace({ message: `'addCategory' - Request failed`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
                this.setState({ error: response.statusText, isSubmitLoading: false })
            }
        }
    }

    /**
     * Handle name change event.
     */
    handleInputNameChange = (event: any) => {
        this.setState({ categoryName: event.target.value, isNameValuePresent: true, error: "" });
    }

    /**
     * Handle description change event.
     */
    handleInputDescriptionChange = (event: any) => {
        this.setState({ categoryDescription: event.target.value, isDescriptionValuePresent: true });
    }

  
    /**
    *Returns text component containing error message for failed name field validation
    *@param {boolean} isValuePresent Indicates whether value is present
    */
    private getRequiredFieldError = (isValuePresent: boolean, t: any) => {
        if (!isValuePresent) {
            return (<Text content={t('fieldRequiredMessage')} className="field-error-message" error size="medium" />);
        }

        return (<></>);
    }

    render() {
        const { t } = this.props;

        return (
            <>
                <div className="tab-container">
                    <div>
                        <Flex hAlign="center" gap="gap.smaller" className="add-toppadding">
                            <Text content={this.state.error} className="field-error-message" error size="medium" />
                        </Flex>
                        <Flex>
                            <Text size="small" content={"*" + t('categoryName')} />
                            <Flex.Item push>
                                {this.getRequiredFieldError(this.state.isNameValuePresent, t)}
                            </Flex.Item>
                        </Flex>
                        <div className="add-form-input">
                            <Input placeholder={t('categoryNamePlaceholder')}
                                fluid required maxLength={50}
                                value={this.state.categoryName}
                                onChange={this.handleInputNameChange}
                            />
                        </div>
                    </div>
                    <div>
                        <Flex className="add-toppadding">
                            <Text size="small" content={"*" + t('categoryDescription')} />
                            <Flex.Item push>
                                {this.getRequiredFieldError(this.state.isDescriptionValuePresent, t)}
                            </Flex.Item>
                        </Flex>
                        <div className="add-form-input">
                            <TextArea placeholder={t('categoryDescriptionPlaceholder')}
                                fluid required maxLength={300}
                                className="response-text-area"
                                value={this.state.categoryDescription}
                                onChange={this.handleInputDescriptionChange}
                            />
                        </div>
                    </div>
                </div>
                <div className="tab-footer">
                    <div>
                        <Flex space="between">
                            <Button icon={<ChevronStartIcon /> }
                                content={t('backButtonText')} text
                                onClick={this.props.onBackButtonClick} />
                            <Flex gap="gap.small">
                                <Button content={t('addButtonTextInManageCategories')} primary
                                    loading={this.state.isSubmitLoading}
                                    disabled={this.state.isSubmitLoading || !this.props.isNewAllowed}
                                    onClick={() => { this.onAddButtonClick(t) }}
                                />
                            </Flex>
                        </Flex>
                    </div>
                </div>
            </>
        );
    }
}

export default withTranslation()(AddCategory)