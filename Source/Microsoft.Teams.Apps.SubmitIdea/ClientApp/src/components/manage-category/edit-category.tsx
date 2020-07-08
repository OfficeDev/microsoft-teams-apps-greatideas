// <copyright file="edit-category.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Flex, Text, Button, Input, TextArea, ChevronStartIcon } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";
import { createBrowserHistory } from "history";
import { updateCategory } from "../../api/category-api";
import { getApplicationInsightsInstance } from "../../helpers/app-insights";
import { SeverityLevel } from "@microsoft/applicationinsights-web";
import { withTranslation, WithTranslation } from "react-i18next";
import { CategoryDetails, ICategoryDetails } from "../models/category";
import { isNullorWhiteSpace } from "../../helpers/helper";

const browserHistory = createBrowserHistory({ basename: "" });

interface IEditCategoryState {
    categoryName: string;
    categoryDescription: string;
    createdBy: string,
    createdOn: Date | undefined,
    isNameValuePresent: boolean,
    isDescriptionValuePresent: boolean,
    error: string,
    isSubmitLoading: boolean
}

interface ICategoryProps extends WithTranslation {
    category: ICategoryDetails,
    teamId: string,
    onBackButtonClick: () => void,
    onSuccess: (operation: string) => void
}

/** Component to edit category details. */
class EditCategory extends React.Component<ICategoryProps, IEditCategoryState> {
    telemetry?: any = null;
    locale?: string | null;
    theme?: string | null;
    userObjectId?: string = "";
    appInsights: any;

    constructor(props: any) {
        super(props);
        this.state = {
            categoryName: props.category.categoryName,
            categoryDescription: props.category.categoryDescription,
            createdBy: props.category.createdBy,
            createdOn: props.category.createdOn,
            isNameValuePresent: true,
            isDescriptionValuePresent: true,
            error: "",
            isSubmitLoading: false
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
     * Handle update changes event.
     */
    onUpdateButtonClick = async (t: any) => {
        if (this.checkIfSubmitAllowed(t)) {
            this.setState({ isSubmitLoading: true });
            let categoryDetail: CategoryDetails = {
                categoryId: this.props.category.categoryId,
                categoryName: this.state.categoryName.trim(),
                categoryDescription: this.state.categoryDescription.trim(),
                createdByUserId: this.state.createdBy,
                modifiedByUserId: this.userObjectId,
                createdOn: this.state.createdOn
            };

            this.appInsights.trackTrace({ message: `'editCategory' - Initiated request`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
            let response = await updateCategory(categoryDetail);
            if (response.status === 200 && response.data) {
                this.appInsights.trackTrace({ message: `'editCategory' - Request success`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
                this.appInsights.trackEvent({ name: `Edit category` }, { User: this.userObjectId, Team: this.props.teamId! });
                this.props.onSuccess("edit");
            }
            else {
                this.appInsights.trackTrace({ message: `'editCategory' - Request failed`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
                this.setState({ error: response.statusText, isSubmitLoading: false })
            }
        }
    }

    /**
     * Validate input fields for update.
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

        return true;
    }

    /**
     * Handle name change event.
     */
    handleInputNameChange = (event: any) => {
        this.setState({ categoryName: event.target.value, isNameValuePresent: true });
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

    /**
    * Renders the component
    */
    public render(): JSX.Element {
        return (
            <div>
                {this.getWrapperPage()}
            </div>
        );
    }

    private getWrapperPage = () => {
        const { t } = this.props;
        return (
            <div >
                <div className="tab-container">
                    <div>
                        <Flex hAlign="center" className="add-toppadding" gap="gap.smaller">
                            <Text content={this.state.error} className="field-error-message" error size="medium" />
                        </Flex>
                        <Flex>
                            <Text size="small" content={"*" + t('categoryName')}/>
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
                            <Button icon={<ChevronStartIcon/>}
                                content={t('backButtonText')} text
                                onClick={this.props.onBackButtonClick} />
                            <Flex gap="gap.small">
                                <Button content={t('saveButtonText')} primary
                                    loading={this.state.isSubmitLoading}
                                    disabled={this.state.isSubmitLoading}
                                    onClick={() => { this.onUpdateButtonClick(t) }}
                                />
                            </Flex>
                        </Flex>
                    </div>
                </div>
            </div>
        );
    }
}

export default withTranslation()(EditCategory)