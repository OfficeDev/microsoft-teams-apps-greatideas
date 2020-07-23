// <copyright file="manage-categories.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Loader, Flex, Text, Image } from "@fluentui/react-northstar";
import { SeverityLevel } from "@microsoft/applicationinsights-web";
import { getApplicationInsightsInstance } from "../../helpers/app-insights";
import * as microsoftTeams from "@microsoft/teams-js";
import { createBrowserHistory } from "history";
import CommandBar from "./category-commandbar";
import CategoryTable from "./category-table";
import { getAllCategories } from "../../api/category-api";
import AddCategory from "./add-new-category";
import EditCategory from "./edit-category";
import DeleteCategory from "./delete-category";
import { WithTranslation, withTranslation } from "react-i18next";
import { ICategoryDetails } from "../models/category";
import "../../styles/curator.css"
let moment = require('moment');

const browserHistory = createBrowserHistory({ basename: "" });

interface ICategoryState {
    loader: boolean;
    categories: ICategoryDetails[];
    selectedCategories: string[];
    filteredCategory: ICategoryDetails[];
    showAddCategory: boolean;
    showEditCategory: boolean;
    editCategory: ICategoryDetails | undefined;
    message: string | undefined;
    showDeleteCategory: boolean;
}

/** Component for displaying on category details. */
class ManageCategory extends React.Component<WithTranslation, ICategoryState> {
    telemetry?: any = null;
    teamId?: string | null;
    userObjectId?: string = "";
    appInsights: any;
    appUrl: string = (new URL(window.location.href)).origin;
    translate: any;

    constructor(props: any) {
        super(props);
        let search = window.location.search;
        let params = new URLSearchParams(search);
        this.telemetry = params.get("telemetry");
        this.teamId = params.get("teamId");

        this.state = {
            loader: true,
            filteredCategory: [],
            categories: [],
            selectedCategories: [],
            showAddCategory: false,
            showEditCategory: false,
            editCategory: undefined,
            message: undefined,
            showDeleteCategory: false
        }
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
            this.getCategory();
        });

    }

    /**
    *Get categories from API
    */
    async getCategory() {
        this.appInsights.trackTrace({ message: `'getCategory' - Initiated request`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
        let category = await getAllCategories();

        if (category.status === 200 && category.data) {
            this.appInsights.trackTrace({ message: `'getCategory' - Request success`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
            console.log(category);
            this.setState({
                categories: category.data,
                filteredCategory: category.data
            });
        }
        else {
            this.appInsights.trackTrace({ message: `'getCategory' - Request failed`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
        }
        this.setState({
            loader: false
        });
    }

    /**
     * Handle back button click.
     */
    onBackButtonClick = () => {
        this.setState({ showAddCategory: false, showEditCategory: false, selectedCategories: [], showDeleteCategory: false });
        this.getCategory();
    }

    /**
    *Filters table as per search text entered by user
    *@param {String} searchText Search text entered by user
    */
    handleSearch = (searchText: string) => {
        if (searchText) {
            let filteredData = this.state.categories.filter(function (category) {
                return category.categoryName ?.toUpperCase().includes(searchText.toUpperCase()) ||
                    category.categoryDescription ?.toUpperCase().includes(searchText.toUpperCase());
            });
            this.setState({ filteredCategory: filteredData });
        }
        else {
            this.setState({ filteredCategory: this.state.categories });
        }
    }

    /**
     * Handle category selection change.
     */
    onCategorySelected = (categoryId: string, isSelected: boolean) => {
        if (isSelected) {
            let selectCategory = this.state.selectedCategories;
            selectCategory.push(categoryId);
            this.setState({
                selectedCategories: selectCategory
            })
        }
        else {
            let filterCategory = this.state.selectedCategories.filter((Id) => {
                return Id !== categoryId;
            });

            this.setState({
                selectedCategories: filterCategory
            })
        }
    }

    /**
    *Navigate to add new category page
    */
    handleAddButtonClick = () => {
        this.setState({ showAddCategory: true });
    }

    /**
    *Navigate to edit category page
    */
    handleEditButtonClick = () => {
        let editCategory = this.state.categories.find(category => category.categoryId === this.state.selectedCategories[0])
        this.setState({ showEditCategory: true, editCategory: editCategory });
    }

    /**
    *Deletes selected categories
    */
    handleDeleteButtonClick = () => {
        this.setState({ showDeleteCategory: true });
    }

    onSuccess = (operation: string) => {
        if (operation === "add") {
            this.setState({ message: this.translate('successAddCategory'), showAddCategory: false, showEditCategory: false, selectedCategories: [], showDeleteCategory: false });
        }
        else if (operation === "delete") {
            this.setState({ message: this.translate('successDeleteCategory'), showAddCategory: false, showEditCategory: false, selectedCategories: [], showDeleteCategory: false });
        }
        else if (operation === "edit") {
            this.setState({ message: this.translate('successEditCategory'), showAddCategory: false, showEditCategory: false, selectedCategories: [], showDeleteCategory: false });
        }
        this.getCategory();
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

    /**
    *Get wrapper for page which acts as container for all child components
    */
    private getWrapperPage = () => {
        const { t } = this.props;
        this.translate = t;
        if (this.state.loader) {
            return (
                <div className="tab-container">
                    <Loader />
                </div>
            );
        } else {

            return (
                <div className="module-container">
                    {(this.state.showAddCategory === false && this.state.showEditCategory === false && !this.state.showDeleteCategory) &&
                        <div className="tab-container">
                            <CommandBar
                                isDeleteEnable={this.state.selectedCategories.length > 0}
                                isEditEnable={this.state.selectedCategories.length > 0 && this.state.selectedCategories.length < 2}
                                onAddButtonClick={this.handleAddButtonClick}
                                onDeleteButtonClick={this.handleDeleteButtonClick}
                                onEditButtonClick={this.handleEditButtonClick}
                                handleTableFilter={this.handleSearch}
                                isAddEnabled={!(this.state.categories.length >= 10)}
                            />
                            <div>
                                {this.state.categories.length !== 0 &&
                                    <CategoryTable showCheckbox={true}
                                        categories={this.state.filteredCategory}
                                        onCheckBoxChecked={this.onCategorySelected}
                                    />
                                }
                            </div>
                            {this.state.categories.length === 0 &&
                                <Flex gap="gap.small" className="margin-top-medium" >
                                    <Flex.Item>
                                        <Image className="icon-size" fluid src={this.appUrl + "/Artifacts/helpIcon.png"} />
                                    </Flex.Item>
                                    <Flex.Item>
                                        <Flex column gap="gap.small" >
                                            <Text weight="bold" content={t('noCategoryFoundText1')} />
                                            <Text content={t('noCategoryFoundText2')} />
                                        </Flex>
                                    </Flex.Item>
                                </Flex>}
                        </div>}
                    {this.state.showAddCategory && <div>
                        <AddCategory
                            isNewAllowed={!(this.state.categories.length >= 10)}
                            categories={this.state.categories}
                            onBackButtonClick={this.onBackButtonClick}
                            teamId={this.teamId!}
                            onSuccess={this.onSuccess}
                        />
                    </div>}
                    {this.state.showEditCategory && <div>
                        <EditCategory
                            category={this.state.editCategory!}
                            onBackButtonClick={this.onBackButtonClick}
                            teamId={this.teamId!}
                            onSuccess={this.onSuccess}
                        />
                    </div>}
                    {(this.state.showAddCategory === false && this.state.showEditCategory === false && !this.state.showDeleteCategory) &&
                        <div className="category-footer">
                            <Flex>
                                {this.state.message !== undefined &&
                                    <Flex vAlign="center" className=" margin-left-large">
                                        <Image className="preview-image-icon" fluid src={this.appUrl + "/Artifacts/categoryIcon.png"} />
                                        <Text className="margin-left-small" content={this.state.message} />
                                    </Flex>}
                                {this.state.categories.length > 0 && <Flex.Item push>
                                    <Text size="small" align="end" content={t('lastUpdatedOn', { time: moment(new Date(this.state.categories[0].timestamp)).format("llll") })} />
                                </Flex.Item>}
                            </Flex>
                        </div>}
                    {this.state.showDeleteCategory && <div>
                        <DeleteCategory
                            categories={this.state.categories}
                            selectedCategories={this.state.selectedCategories}
                            onBackButtonClick={this.onBackButtonClick}
                            teamId={this.teamId!}
                            onSuccess={this.onSuccess}
                        />
                    </div>}
                </div>
            );
        }
    }
}

export default withTranslation()(ManageCategory);
