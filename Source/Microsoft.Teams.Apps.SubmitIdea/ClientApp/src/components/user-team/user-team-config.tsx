// <copyright file="user-team-config.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Flex, Text, Input, Dropdown } from "@fluentui/react-northstar";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import { ICategoryDetails } from "../models/category";
import { getAllCategories } from "../../api/category-api";
import { submitConfigCategories, getConfigCategories, updateConfigCategories } from "../../api/teams-config-tab-api";

export interface IConfigState {
    url: string;
    tabName: string;
    category: string;
    loading: boolean,
    selectedCategoryList: Array<ICategoryDetails>,
    categories: Array<ICategoryDetails>,
    theme: string;
    selectedPreference: ITeamConfigDetails | undefined;
}

interface ITeamConfigDetails {
    categories: string;
    teamId: string;
}

class UserTeamConfig extends React.Component<WithTranslation, IConfigState> {
    localize: TFunction;
    userObjectId: string = "";
    teamId: string = "";
    appInsights: any;
    telemetry: string | undefined = "";

    constructor(props: any) {
        super(props);
        this.localize = this.props.t;
        this.state = {
            url: this.getBaseUrl() + "/team-ideas?theme={theme}&locale={locale}&teamId={teamId}&tenant={tid}",
            tabName: "",
            category: "",
            categories: [],
            selectedCategoryList: [],
            loading: true,
            theme: "",
            selectedPreference: undefined
        }
    }

    private getBaseUrl() {
        return window.location.origin;
    }

    public componentDidMount() {
        microsoftTeams.initialize();
        microsoftTeams.getContext((context) => {
            this.userObjectId = context.userObjectId!;
            this.teamId = context.teamId!;
            this.setState({ theme: context.theme! });
            this.getCategory();
        });

        microsoftTeams.settings.registerOnSaveHandler(async (saveEvent) => {
            let categoryList = this.state.selectedCategoryList.map(x => x.categoryId).join(";");
            let configureDetails: ITeamConfigDetails = {
                teamId: this.teamId,
                categories: categoryList
            }

            let response;
            if (this.state.selectedPreference) {
                response = await updateConfigCategories(configureDetails);
            }
            else {
                response = await submitConfigCategories(configureDetails);
            }

            if (response.status === 200 && response.data) {
                microsoftTeams.settings.setSettings({
                    entityId: "TeamIdeas",
                    contentUrl: this.state.url,
                    websiteUrl: this.state.url,
                    suggestedDisplayName: this.state.tabName,
                });
                saveEvent.notifySuccess();
            }
        });
    }


    /**
   *Get preferences from API
   */
    async getCategoryPreferences() {
        let result = await getConfigCategories(this.teamId);
        let category: Array<ICategoryDetails> = [];
        if (result) {
            result.data.categories ?.split(';').forEach((value) => {
                let matchedData = this.state.categories.find(element => element.categoryId === value);
                if (matchedData) {
                    category.push({
                        categoryId: matchedData.categoryId,
                        categoryName: matchedData.categoryName,
                        categoryDescription: matchedData.categoryDescription,
                        createdByUserId: matchedData.createdByUserId,
                        createdOn: matchedData.createdOn,
                        modifiedByUserId: matchedData.modifiedByUserId,
                        timestamp: matchedData.timestamp
                    });
                }
            });

            this.setState({
                selectedCategoryList: category,
                selectedPreference: result.data
            });
        }
        this.setState({
            loading: false
        });
    }


    /**
    *Get categories from API
    */
    async getCategory() {
        let category = await getAllCategories();

        if (category.status === 200 && category.data) {
            this.setState({
                categories: category.data,
            });

            this.getCategoryPreferences();
        }

        this.setState({
            loading: false
        });
    }

    /**
   *Sets state of tagsList by removing category using its index.
   *@param index Index of category to be deleted.
   */
    onCategoryRemoveClick = (index: number) => {
        let categories = this.state.selectedCategoryList;
        categories.splice(index, 1);
        this.setState({ selectedCategoryList: categories });
    }

    onTabNameChange = (value: string) => {

        this.setState({ tabName: value });

        if (this.state.selectedCategoryList.length > 0 && value) {
            microsoftTeams.settings.setValidityState(true);
        }
        else {
            microsoftTeams.settings.setValidityState(false);
        }
    }

    getA11ySelectionMessage = {
        onAdd: item => {
            if (item) {
                let selectedCategories = this.state.selectedCategoryList;
                let category = this.state.categories.find(category => category.categoryId === item.key);
                if (category) {
                    selectedCategories.push(category);
                    this.setState({ selectedCategoryList: selectedCategories });
                    if (selectedCategories.length > 0 && this.state.tabName) {
                        microsoftTeams.settings.setValidityState(true);
                    }
                    else {
                        microsoftTeams.settings.setValidityState(false);
                    }
                }
            }
            return "";
        },
        onRemove: item => {
            let categoryList = this.state.selectedCategoryList;
            let filterCategories = categoryList.filter(category => category.categoryId !== item.key);
            this.setState({ selectedCategoryList: filterCategories });
            if (filterCategories.length > 0 && this.state.tabName) {
                microsoftTeams.settings.setValidityState(true);
            }
            else {
                microsoftTeams.settings.setValidityState(false);
            }
            return "";
        }
    }


    public render(): JSX.Element {
        if (!this.state.loading) {
            return (
                <div className="config-container">
                    <Flex gap="gap.small" column>
                        <Flex.Item>
                            <>
                                <Text size="small" content={"*" + this.localize("tabName")} />
                                <Input fluid placeholder={this.localize("tabNamePlaceholder")} value={this.state.tabName} onChange={(event: any) => this.onTabNameChange(event.target.value)} />
                            </>
                        </Flex.Item>
                        <Flex.Item>
                            <>
                                <div className="add-toppadding"><Text size="small" content={"*" + this.localize("category")} /></div>
                                <Dropdown
                                    items={this.state.categories.map(category => {
                                        return { key: category.categoryId, header: category.categoryName }
                                    })}
                                    multiple
                                    search
                                    fluid
                                    placeholder={this.localize("categoryDropdownPlaceholder")}
                                    getA11ySelectionMessage={this.getA11ySelectionMessage}
                                    value={this.state.selectedCategoryList.map(category => {
                                        return { key: category.categoryId, header: category.categoryName }
                                    })}
                                />
                            </>
                        </Flex.Item>
                    </Flex>
                </div>
            );
        }
        else {
            return <></>;
        }

    }
}

export default withTranslation()(UserTeamConfig)