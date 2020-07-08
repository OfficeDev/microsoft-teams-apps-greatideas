// <copyright file="title-bar-teams.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import FilterBar from "../filter-bar/filter-bar";
import CommandBar from "../filter-bar/command-bar";
import { ICheckBoxItem } from "../filter-bar/filter-bar";
import { getTeamConfiguredTags, getTeamAuthorsData, getTeamConfiguredCategoriess } from "../../api/idea-api";

interface IFilterBarProps {
    onTypeCheckboxStateChange: (currentValues: Array<ICheckBoxItem>) => void
    onSharedByCheckboxStateChange: (currentValues: Array<ICheckBoxItem>) => void
    onSearchInputChange: (searchString: string) => void;
    onSortByChange: (selectedValue: string) => void;
    onFilterSearchChange: (searchText: string) => void;
    onTagsStateChange: (currentValues: Array<ICheckBoxItem>) => void;
    searchFilterPostsUsingAPI: () => void;
    onFilterClear: (isFilterOpened: boolean) => void;
    commandBarSearchText: string;
    showAddNewButton: boolean;
    hideFilterbar: boolean;
}

interface IFilterBarState {
    isOpen: boolean;
    sharedByAuthorList: Array<string>;
    tagsList: Array<string>;
    showSolidFilter: boolean;
    categoryList: Array<string>;
}

class TitleBar extends React.Component<IFilterBarProps, IFilterBarState> {
    teamId: string;

    constructor(props: IFilterBarProps) {
        super(props);
        this.teamId = "";
        this.state = {
            isOpen: false,
            sharedByAuthorList: [],
            tagsList: [],
            categoryList: [],
            showSolidFilter: false
        }
    }

    componentDidMount() {
        microsoftTeams.initialize();
        microsoftTeams.getContext((context: microsoftTeams.Context) => {
            this.teamId = context.teamId!;
            this.getTeamConfigTags();
            this.getTeamAuthors();
            this.getTeamCategories();
        });
    }

    componentWillReceiveProps(nextProps: IFilterBarProps) {
        if (nextProps.hideFilterbar !== this.props.hideFilterbar) {
            if (nextProps.hideFilterbar === true) {
                this.setState({ isOpen: false });
                this.getTeamAuthors();
                this.getTeamConfigTags();
                this.getTeamCategories();
            }
        }
    }

	/**
    * Fetch list of authors from API
    */
    getTeamAuthors = async () => {
        let response = await getTeamAuthorsData(this.teamId);
        if (response.status === 200 && response.data) {
            this.setState({
                sharedByAuthorList: response.data
            });
        }
    }

	/**
    * Fetch list of tags from API
    */
    getTeamConfigTags = async () => {
        let response = await getTeamConfiguredTags(this.teamId);
        if (response.status === 200 && response.data) {
            this.setState({
                tagsList: response.data
            });
        }
    }

    /**
    * Fetch list of tags from API
    */
    getTeamCategories = async () => {
        let response = await getTeamConfiguredCategoriess(this.teamId);
        if (response.status === 200 && response.data) {
            this.setState({
                categoryList: response.data
            });
        }
    }

    changeOpenState = () => {
        this.setState({ showSolidFilter: !this.state.showSolidFilter });
        this.setState({ isOpen: !this.state.isOpen });
        this.props.onFilterClear(!this.state.isOpen);
    }

	/**
	* Renders the component
	*/
    public render(): JSX.Element {
        return (
            <>
                <CommandBar
                    onFilterButtonClick={this.changeOpenState}
                    onSearchInputChange={this.props.onSearchInputChange}
                    showSolidFilterIcon={this.state.showSolidFilter}
                    searchFilterPostsUsingAPI={this.props.searchFilterPostsUsingAPI}
                    commandBarSearchText={this.props.commandBarSearchText}
                    showAddNewButton={this.props.showAddNewButton}
                />

                <FilterBar
                    tagsList={this.state.tagsList}
                    categoryList={this.state.categoryList}
                    onFilterSearchChange={this.props.onFilterSearchChange}
                    onSortByStateChange={this.props.onSortByChange}
                    sharedByAuthorList={this.state.sharedByAuthorList}
                    isVisible={this.state.isOpen}
                    onFilterBarCloseClick={this.changeOpenState}
                    onSharedByCheckboxStateChange={this.props.onSharedByCheckboxStateChange}
                    onTypeCheckboxStateChange={this.props.onTypeCheckboxStateChange}
                    onTagsStateChange={this.props.onTagsStateChange} />
            </>
        )
    }
}

export default TitleBar;