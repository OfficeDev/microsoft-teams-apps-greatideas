// <copyright file="delete-category.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Text, Flex, Button, ChevronStartIcon, Alert } from "@fluentui/react-northstar";
import { createBrowserHistory } from "history";
import * as microsoftTeams from "@microsoft/teams-js";
import { SeverityLevel } from "@microsoft/applicationinsights-web";
import { getApplicationInsightsInstance } from "../../helpers/app-insights";
import { deleteSelectedCategories } from "../../api/category-api";
import { withTranslation, WithTranslation } from "react-i18next";
import { CategoryDetails } from "../models/category";


const browserHistory = createBrowserHistory({ basename: "" });

interface ICategoryProps extends WithTranslation {
    categories: CategoryDetails[],
    selectedCategories: string[],
    teamId: string,
    onBackButtonClick: () => void,
    onSuccess: (operation: string) => void
}

interface ICategoryState {
    error: string,
    isSubmitLoading: boolean;
}

class DeleteCategory extends React.Component<ICategoryProps, ICategoryState> {
    telemetry?: any = null;
    appInsights: any;
    userObjectId?: string = "";

    constructor(props: any) {
        super(props);

        this.state = {
            error: "",
            isSubmitLoading: false,
        }

        let search = window.location.search;
        let params = new URLSearchParams(search);
        this.telemetry = params.get("telemetry");
        this.appInsights = {};
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
    * Handles delete category event.
    */
    onDeleteButtonClick = async () => {
        const { t } = this.props;
        this.appInsights.trackTrace({ message: `Delete category - Initiated request`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
        this.setState({ isSubmitLoading: true });
        let categoryIds = this.props.selectedCategories.join(',');
        let deletionResult = await deleteSelectedCategories(categoryIds);
        if (deletionResult.status === 200 && deletionResult.data) {
            this.appInsights.trackTrace({ message: `'Delete category' - Request success`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
            this.appInsights.trackEvent({ name: `Delete category` }, { User: this.userObjectId, Team: this.props.teamId! });
            this.props.onSuccess("delete");
        }
        else if (deletionResult.status === 200 && !deletionResult.data) {
            this.appInsights.trackTrace({ message: `'Delete category' - Request failed`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
            this.setState({ error: t('categoryDeleteValidationMessage'), isSubmitLoading: false })
        }
        else {
            this.appInsights.trackTrace({ message: `'Delete category' - Request failed`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
            this.setState({ error: deletionResult.statusText, isSubmitLoading: true })
        }
    }

    /**
    * Returns categories that are to be deleted.
    */
    deleteContent = () => {
        let categories = this.props.categories.filter((category) => {
            return this.props.selectedCategories.includes(category.categoryId!);
        });

        let categoriesTableRows = categories.map((value) => {
            return (
                <Alert info className="tiny-top-margin"
                    content={<Flex column padding="padding.medium"><><Text content={value.categoryName} weight="bold" className="word-break" title={value.categoryName} />
                        <Text content={value.categoryDescription} className="word-break" title={value.categoryDescription} /></></Flex>}
                />
            )
        });

        return categoriesTableRows;
    }

    render() {
        const { t } = this.props;

        return (
            <>
                <div className="tab-container">
                    <Text weight="semibold" content={t('categoryDeleteConfirmationMessageText')} />
                    <div className="top-spacing">{this.deleteContent()}</div>
                </div>
                <div className="tab-footer">
                    <div>
                        <Flex space="between">
                            <Button icon={<ChevronStartIcon />}
                                content={t('backButtonText')} text
                                onClick={this.props.onBackButtonClick} />
                            <Flex gap="gap.small">
                                <Button content={t('deleteButtonText')} primary
                                    loading={this.state.isSubmitLoading}
                                    onClick={() => { this.onDeleteButtonClick() }}
                                />
                            </Flex>
                        </Flex>
                    </div>
                    <div className="tab-footer-delete-category">
                        <Flex hAlign="center">
                            <Text content={this.state.error} className="field-error-message" error size="medium" />
                        </Flex>
                    </div>
                </div>
            </>
        );
    }
}

export default withTranslation()(DeleteCategory)