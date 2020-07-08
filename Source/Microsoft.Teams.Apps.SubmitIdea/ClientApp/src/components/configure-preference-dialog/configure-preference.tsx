// <copyright file="configure-preference" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Flex, Text, Dropdown, RadioGroup, Loader, Button } from "@fluentui/react-northstar";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import { getPreferenceDetails } from "../../helpers/helper";
import { getAllCategories } from "../../api/category-api";
import Resources from '../../constants/resources';
import "../../styles/configure-preferences.css";

interface IConfigurePreferencesProps extends WithTranslation {
    configurePreferencesDetails: IConfigurePreferencesDetails;
    changeDialogOpenState: (isOpen: boolean) => void;
}

export interface IConfigurePreferencesState {
    loading: boolean,
    selectedCategoryList: Array<any>,
    allCategories: Array<any>,
    selectedDigestFrequency: string | undefined;
    isSubmitLoading: boolean;
    isCategoryPresent: boolean;
    configurePreferencesDetails: IConfigurePreferencesDetails;
}

interface IConfigurePreferencesDetails {
    categories: string;
    digestFrequency: string;
    teamId: string;
}

class ConfigurePreferences extends React.Component<IConfigurePreferencesProps, IConfigurePreferencesState> {
    localize: TFunction;
    userObjectId: string = "";
    teamId: string = "";
    appInsights: any;
    telemetry: string | undefined = "";
    theme: string | undefined;
    constructor(props: any) {
        super(props);
        this.localize = this.props.t;
        this.teamId = "";
        this.state = {
            allCategories: [],
            selectedDigestFrequency: Resources.weeklyDigestFrequencyText,
            selectedCategoryList: [],
            loading: true,
            isSubmitLoading: false,
            isCategoryPresent: true,
            configurePreferencesDetails: { ...this.props.configurePreferencesDetails }
        }
    }

    /**
   * Used to initialize Microsoft Teams sdk
   */
    async componentDidMount() {
        microsoftTeams.initialize();
        microsoftTeams.getContext(async (context) => {
            this.teamId = context.teamId!;
            this.theme = context.theme;
            this.setState({
                loading: true
            });
        });
        await this.getCategory();
        await this.getPreferences();
    }

    /**
    *Get preferences from API
    */
    async getPreferences() {
        let result = await getPreferenceDetails(this.teamId);
        let category: Array<any> = [];
        if (result !== undefined) {
            result.categories.forEach((value) => {
                let matchedData = this.state.allCategories.find(element => element.key === value);
                if (matchedData) {
                    category.push({
                        key: matchedData.key,
                        header: matchedData.header,
                    });
                }
            });

            this.setState({
                selectedCategoryList: category,
                selectedDigestFrequency: result.frequency
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
        this.setState({
            loading: true
        });
        let category = await getAllCategories();
        if (category.status === 200 && category.data) {
            let categoryDetails: any[] = [];
            category.data.forEach((value) => {
                categoryDetails.push({
                    key: value.categoryId,
                    header: value.categoryName,
                });
            });
            this.setState({
                allCategories: categoryDetails,
            });
        }
        this.setState({
            loading: false
        });
    }

    /**
    * Method to submit and save preferences details.
    */
    onSubmitClick = async () => {
        if (this.state.selectedCategoryList.length === 0) {
            this.setState({ isCategoryPresent: false });
            return;
        }
        this.setState({
            isSubmitLoading: true
        });
        let configureDetails = this.state.configurePreferencesDetails;
        let categoryUniqueIds: Array<any> = [];
        this.state.selectedCategoryList.forEach((value) => {
            categoryUniqueIds.push(value.key);
        });
        configureDetails.digestFrequency = this.state.selectedDigestFrequency!;
        configureDetails.categories = categoryUniqueIds.join(';');
        configureDetails.teamId = this.teamId;
        let toBot =
        {
            configureDetails,
            command: Resources.submitPreferencesTaskModule
        };
        this.setState({
            isSubmitLoading: false
        });
        microsoftTeams.tasks.submitTask(toBot);
    }

    /**
   *Sets state of selectedCategoryList by removing category using its index.
   *@param index Index of category to be deleted.
   */
    onCategoryRemoveClick = (index: number) => {
        let categories = this.state.selectedCategoryList;
        categories.splice(index, 1);
        this.setState({ selectedCategoryList: categories });
    }

    /**
    * Method to get selected digest frequency value in state.
    * @param e event parameter.
    * @param props event parameter.
    */
    getDigestFrequency = (e: any, props: any) => {
        this.setState({
            selectedDigestFrequency: props.value
        })
    }

    /**
    * Digest frequency radio button details.
    */
    getItems() {
        return [
            {
                name: Resources.digestFrequencyRadioName,
                key: Resources.weeklyDigestFrequencyText,
                label: this.localize('weeklyFrequencyText'),
                value: Resources.weeklyDigestFrequencyText,
            },
            {
                name: Resources.digestFrequencyRadioName,
                key: Resources.monthlyDigestFrequencyText,
                label: this.localize('monthlyFrequencyText'),
                value: Resources.monthlyDigestFrequencyText,
            },
        ]
    }

    getA11ySelectionMessage = {
        onAdd: item => {
            if (item) {
                let selectedCategories = this.state.selectedCategoryList;
                let category = this.state.allCategories.find(category => category.key === item.key);
                selectedCategories.push(category);
                this.setState({ selectedCategoryList: selectedCategories, isCategoryPresent: true });
            }
            return "";
        },
        onRemove: item => {
            let categoryList = this.state.selectedCategoryList;
            let filterCategories = categoryList.filter(category => category.key !== item.key);
            this.setState({ selectedCategoryList: filterCategories });
            return "";
        }
    }

    /**
    *Returns text component containing error message for failed name field validation
    *@param {boolean} isValuePresent Indicates whether value is present
    */
    private getRequiredFieldError = (isValuePresent: boolean) => {
        if (!isValuePresent) {
            return (<Text content={this.localize("fieldRequiredMessage")} className="field-error-message" error size="medium" />);
        }
        return (<></>);
    }

    public render(): JSX.Element {
        if (!this.state.loading) {
            return (
                <div className="configure-preferences-div">
                    <Flex gap="gap.smaller">
                        <div className="top-spacing">
                            <Text size="small" content={this.localize("digestFrequencyLabel")} />
                        </div>
                    </Flex>
                    <Flex gap="gap.smaller" className="frequency-radio">
                        <RadioGroup
                            vertical
                            items={this.getItems()}
                            defaultCheckedValue={this.state.selectedDigestFrequency}
                            checkedValue={this.state.selectedDigestFrequency}
                            onCheckedValueChange={(e: any, props: any) => this.getDigestFrequency(e, props)}
                        />
                    </Flex>
                    <Flex gap="gap.smaller" className="add-toppadding">
                        <Text size="small" content={"*" + this.localize("categoriesLabel")} />
                        <Flex.Item push>
                            {this.getRequiredFieldError(this.state.isCategoryPresent)}
                        </Flex.Item>
                    </Flex>
                    <Flex vAlign="center">
                        <Flex.Item align="start" grow>
                            <Dropdown className="top-space"
                                items={this.state.allCategories}
                                multiple
                                search
                                fluid
                                placeholder={this.localize("categoriesPlaceholder")}
                                getA11ySelectionMessage={this.getA11ySelectionMessage}
                                noResultsMessage={this.localize("noCategoryMatchFoundText")}
                                value={this.state.selectedCategoryList}
                            />
                        </Flex.Item>
                    </Flex>
                    <div className="tab-footer">
                        <Flex hAlign="end" >
                            <Button primary loading={this.state.isSubmitLoading}
                                disabled={this.state.isSubmitLoading}
                            content={this.localize("Save")} onClick={this.onSubmitClick} />
                        </Flex>
                    </div>
                </div>

            );
        }
        else {
            return (
                <div className="dialog-container-div-preferences">
                    <Loader className="preference-loader" />
                </div>
            )
        }

    }
}

export default withTranslation()(ConfigurePreferences)