// <copyright file="title-bar.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import FilterBar from "./ideas-filter-bar";
import CommandBar from "./command-bar";
import { ICheckBoxItem } from "./ideas-filter-bar"
import { getAuthors, getTags, getCategories } from "../../api/idea-api";

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
    hideFilterbar: boolean;
    showAddNewButton: boolean;
    botId: string;
}

interface IFilterBarState {
    isOpen: boolean;
    sharedByAuthorList: Array<string>;
    tagsList: Array<string>;
    categoryList: Array<string>;
    showSolidFilter: boolean;
}

class TitleBar extends React.Component<IFilterBarProps, IFilterBarState> {
    constructor(props: IFilterBarProps) {
        super(props);

        this.state = {
            isOpen: false,
            sharedByAuthorList: [],
            tagsList: [],
            categoryList: [],
            showSolidFilter: false
        }
    }

    componentDidMount() {
        this.getAuthors();
        this.getTags();
        this.getCategories();
    }

    componentWillReceiveProps(nextProps: IFilterBarProps) {
        if (nextProps.hideFilterbar !== this.props.hideFilterbar) {
            if (nextProps.hideFilterbar === true) {
                this.setState({ isOpen: false });
                this.getAuthors();
                this.getTags();
                this.getCategories();
            }
        }
    }

	/**
    * Fetch list of authors from API
    */
    getAuthors = async () => {
        let response = await getAuthors();
        if (response.status === 200 && response.data) {
            this.setState({
                sharedByAuthorList: response.data.map((author: string) => { return author.trim() })
            });
        }
    }

	/**
    * Fetch list of tags from API
    */
    getTags = async () => {
        let response = await getTags();
        if (response.status === 200 && response.data) {
            this.setState({
                tagsList: response.data
            });
        }
    }

    /**
    * Fetch list of tags from API
    */
    getCategories = async () => {
        let response = await getCategories();
        if (response.status === 200 && response.data) {
            this.setState({
                categoryList: response.data
            });
        }
    }

	/**
    * Sets state to show/hide filter bar
    */
    onOpenStateChange = () => {
        this.setState({ showSolidFilter: !this.state.showSolidFilter, isOpen: !this.state.isOpen });
        this.props.onFilterClear(!this.state.isOpen);
    }

	/**
	* Renders the component
	*/
    public render(): JSX.Element {
        return (
            <>
                <CommandBar
                    onFilterButtonClick={this.onOpenStateChange}
                    onSearchInputChange={this.props.onSearchInputChange}
                    showSolidFilterIcon={this.state.showSolidFilter}
                    searchFilterPostsUsingAPI={this.props.searchFilterPostsUsingAPI}
                    commandBarSearchText={this.props.commandBarSearchText}
                    botId={this.props.botId}
                    showAddNewButton={this.props.showAddNewButton}
                />
                <FilterBar
                    tagsList={this.state.tagsList}
                    categoryList={this.state.categoryList}
                    onFilterSearchChange={this.props.onFilterSearchChange}
                    onSortByStateChange={this.props.onSortByChange}
                    sharedByAuthorList={this.state.sharedByAuthorList}
                    isVisible={this.state.isOpen}
                    onFilterBarCloseClick={this.onOpenStateChange}
                    onSharedByCheckboxStateChange={this.props.onSharedByCheckboxStateChange}
                    onTypeCheckboxStateChange={this.props.onTypeCheckboxStateChange}
                    onTagsStateChange={this.props.onTagsStateChange} />
            </>
        )
    }
}

export default TitleBar;