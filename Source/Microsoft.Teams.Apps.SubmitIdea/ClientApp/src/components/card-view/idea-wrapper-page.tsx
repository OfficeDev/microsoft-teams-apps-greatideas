﻿// <copyright file="discover-wrapper-page.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Loader } from "@fluentui/react-northstar";
import Card from "./card";
import NoPostAddedPage from "./no-post-added-page";
import FilterNoPostContentPage from "./filter-no-post-content-page";
import TitleBar from "../filter-bar/title-bar";
import { Container, Col, Row } from "react-bootstrap";
import * as microsoftTeams from "@microsoft/teams-js";
import { getAllIdeas, getUserVotes, getFilteredPosts, filterTitleAndTags } from "../../api/idea-api";
import { generateColor } from "../../helpers/helper";
import NotificationMessage from "../notification-message/notification-message";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import { ICheckBoxItem } from "../filter-bar/ideas-filter-bar";
import Resources from "../../constants/resources";
import InfiniteScroll from 'react-infinite-scroller';
import { getBotSetting } from "../../api/setting-api";
import { ApprovalStatus } from "../models/idea";

import 'bootstrap/dist/css/bootstrap.min.css';
import "../../styles/site.css";
import "../../styles/card.css";

export interface IDiscoverPost {
    ideaId: string;
    type: string;
    category: string;
    categoryId: string;
    title: string;
    description: string;
    contentUrl: string;
    tags: string;
    createdDate: Date;
    createdByName: string;
    createdByObjectId: string;
    updatedDate: Date;
    totalVotes: number;
    isVotedByUser?: boolean;
    isCurrentUserPost?: boolean;
    avatarBackgroundColor: string;
    status: number;
}

export interface IUserVote {
    postId: string;
    userId: string;
}

interface ICardViewState {
    loader: boolean;
    resourceStrings: any;
    discoverPosts: Array<IDiscoverPost>;
    discoverSearchPosts: Array<IDiscoverPost>;
    alertMessage: string;
    alertType: number;
    showAlert: boolean;
    searchText: string;
    showNoPostPage: boolean;
    infiniteScrollParentKey: number;
    isFilterApplied: boolean;
    isPageInitialLoad: boolean;
    pageLoadStart: number;
    hasMorePosts: boolean;
    initialPosts: Array<IDiscoverPost>;
}

class IdeaWrapperPage extends React.Component<WithTranslation, ICardViewState> {

    localize: TFunction;
    selectedSharedBy: Array<ICheckBoxItem>;
    selectedPostType: Array<ICheckBoxItem>;
    selectedTags: Array<ICheckBoxItem>;
    selectedSortBy: string;
    filterSearchText: string;
    allPosts: Array<IDiscoverPost>;
    loggedInUserObjectId: string;
    teamId: string;
    authorAvatarBackground: Array<any>;
    botId: string = "";
    telemetry?: string = "";

    constructor(props: any) {
        super(props);
        this.localize = this.props.t;
        this.selectedSharedBy = [];
        this.selectedPostType = [];
        this.selectedTags = [];
        this.selectedSortBy = "";
        this.filterSearchText = "";
        this.allPosts = [];
        this.loggedInUserObjectId = "";
        this.teamId = "";
        this.authorAvatarBackground = [];

        this.state = {
            loader: true,
            discoverPosts: [],
            discoverSearchPosts: [],
            resourceStrings: {},
            alertMessage: "",
            alertType: 0,
            showAlert: false,
            searchText: "",
            showNoPostPage: false,
            isFilterApplied: false,
            infiniteScrollParentKey: 0,
            isPageInitialLoad: true,
            pageLoadStart: -1,
            hasMorePosts: true,
            initialPosts: [],
        }
    }

    /**
    * Used to initialize Microsoft Teams sdk
    */
    async componentDidMount() {
        this.initDiscoverPosts();
        microsoftTeams.initialize();
        microsoftTeams.getContext((context: microsoftTeams.Context) => {
            this.loggedInUserObjectId = context.userObjectId!;

            this.getBotSetting();
        });
    }

    /**
    * Fetch posts for initializing grid
    */
    initDiscoverPosts = async () => {
        let response = await getAllIdeas(0);
        if (response.status === 200 && response.data) {
            this.setState({
                initialPosts: response.data,
                loader: false
            })
        }
    }

    /**
    * Get comma separated selected filter entities string.
    * @param filterEntity Array of selected filter entities.
    */
    private getFilterString(filterEntity: Array<string>) {
        return filterEntity.length > 1 ? filterEntity.join(";") : filterEntity.length == 1 ? filterEntity.join(";") + ";" : "";
    }

    /**
    * Get filtered posts based on selected checkboxes.
    * @param pageCount Page count for which next set of posts needs to be fetched
    */
    getFilteredDiscoverPosts = async (pageCount: number) => {
        let postTypes = this.selectedPostType.map((postType: ICheckBoxItem) => { return postType.id.toString().trim() });
        let postTypesString = encodeURI(this.getFilterString(postTypes));
        let authors = this.selectedSharedBy.map((authors: ICheckBoxItem) => { return authors.title.trim() });
        let authorsString = encodeURI(this.getFilterString(authors));
        let tags = this.selectedTags.map((tag: ICheckBoxItem) => { return tag.title.trim() });
        let tagsString = encodeURI(this.getFilterString(tags));

        let response = await getFilteredPosts(postTypesString, authorsString, tagsString, this.selectedSortBy, "", pageCount);
        if (response.status === 200 && response.data) {
            if (!response.data.length) {
                this.setState({
                    hasMorePosts: false,
                });
            }

            response.data.map((post: IDiscoverPost) => {
                let searchedAuthor = this.authorAvatarBackground.find((author) => author.id === post.createdByObjectId);
                if (searchedAuthor) {
                    post.avatarBackgroundColor = searchedAuthor.color;
                }
                else {
                    let color = generateColor();
                    this.authorAvatarBackground.push({ id: post.createdByObjectId, color: color });
                    post.avatarBackgroundColor = color;
                }

                if (post.createdByObjectId === this.loggedInUserObjectId) {
                    post.isCurrentUserPost = true;
                }
                else {
                    post.isCurrentUserPost = false;
                }

                this.allPosts.push(post);
            });

            if (response.data.count !== 0) {
                this.setState({
                    isPageInitialLoad: false,
                });
            }
            else {
                this.setState({
                    showNoPostPage: true,
                    isPageInitialLoad: false
                })
            }
            this.getUserVotes();
        }
    }

    /**
    * Reset app user selected filters
    */
    resetAllFilters = () => {
        this.selectedSortBy = Resources.sortBy[0].id;
        this.selectedSharedBy = [];
        this.selectedPostType = [];
        this.selectedTags = [];
        this.filterSearchText = "";
    }

    /**
    * Fetch posts for Team tab from API
    * @param pageCount Page count for which next set of posts needs to be fetched
    */
    getDiscoverPosts = async (pageCount: number) => {
        this.resetAllFilters();
        let response = await getAllIdeas(pageCount);
        if (response.status === 200 && response.data) {
            if (response.data.length < 50) {
                this.setState({
                    hasMorePosts: false
                });
            }
            else {
                this.setState({
                    hasMorePosts: true
                });
            }

            response.data.map((post: IDiscoverPost) => {
                let searchedAuthor = this.authorAvatarBackground.find((author) => author.id === post.createdByObjectId);
                if (searchedAuthor) {
                    post.avatarBackgroundColor = searchedAuthor.color;
                }
                else {
                    let color = generateColor();
                    this.authorAvatarBackground.push({ id: post.createdByObjectId, color: color });
                    post.avatarBackgroundColor = color;
                }

                if (post.createdByObjectId === this.loggedInUserObjectId) {
                    post.isCurrentUserPost = true;
                }
                else {
                    post.isCurrentUserPost = false;
                }

                this.allPosts.push(post);
            });

            if (response.data.count === 0) {
                this.setState({
                    showNoPostPage: true
                })
            }
            this.getUserVotes();
        }

        this.setState({
            searchText: "",
            isPageInitialLoad: false
        });
    }

    /**
    *Sets state for showing alert notification.
    *@param content Notification message
    *@param type Boolean value indicating 1- Success 2- Error
    */
    showAlert = (content: string, type: number) => {
        this.setState({ alertMessage: content, alertType: type, showAlert: true }, () => {
            setTimeout(() => {
                this.setState({ showAlert: false })
            }, 4000);
        });
    }

    /**
    *Sets state for hiding alert notification.
    */
    hideAlert = () => {
        this.setState({ showAlert: false })
    }

    /**
    * Fetch user votes from API
    */
    getUserVotes = async () => {
        let response = await getUserVotes();
        if (response.status === 200 && response.data) {
            for (let i = 0; i < response.data.length; i++) {
                this.allPosts.map((post: IDiscoverPost) => {
                    if (post.ideaId === response.data[i].ideaId) {
                        post.isVotedByUser = true;

                        if (post.totalVotes === 0) {
                            post.totalVotes = 1;
                        }
                    }
                })
            }

            this.onFilterSearchTextChange(this.filterSearchText);
        }

        this.setState({
            loader: false
        });
    }

    /**
    *Removes selected blog post from page
    *@param postId Id of post which needs to be deleted
    *@param isSuccess Boolean indication whether operation succeeded
    */
    handleDeleteButtonClick = (postId: string, isSuccess: boolean) => {
        if (isSuccess) {
            this.showAlert(this.localize("postDeletedSuccess"), 1);
            this.onFilterSearchTextChange(this.filterSearchText);
        }
        else {
            this.showAlert(this.localize("postDeletedError"), 2);
        }
    }

    /**
    *Deletes selected blog post
    *@param isSuccess Boolean indication whether operation succeeded
    *@param message Message to be displayed in notification
    */
    handleUserPrivatePostButtonClick = async (isSuccess: boolean, message?: string) => {
        if (isSuccess) {
            this.showAlert(this.localize("addPostToPrivateListSuccess"), 1);
        }
        else {
            if (message) {
                this.showAlert(message, 2);
            }
            else {
                this.showAlert(this.localize("addPrivatePostError"), 2);
            }
        }
    }

    /**
    *Invoked by Infinite scroll component when user scrolls down to fetch next set of posts
    *@param pageCount Page count for which next set of posts needs to be fetched
    */
    loadMorePosts = (pageCount: number) => {
        if (!this.filterSearchText.trim().length) {
            if (this.state.searchText.trim().length) {
                this.searchFilterPostUsingAPI(pageCount);
            }
            else if (this.state.isFilterApplied) {
                this.getFilteredDiscoverPosts(pageCount);
            }
            else {
                this.getDiscoverPosts(pageCount);
            }
        }
    }

    onNewIdeaAdded = () => {
        this.getDiscoverPosts(0);
    }

    /**
    *Set state of search text as per user input change
    *@param searchText Search text entered by user
    */
    handleSearchInputChange = async (searchText: string) => {
        this.setState({
            searchText: searchText
        });

        if (searchText.length === 0) {
            this.setState({
                isPageInitialLoad: true,
                pageLoadStart: -1,
                infiniteScrollParentKey: this.state.infiniteScrollParentKey + 1,
                discoverPosts: [],
                hasMorePosts: true
            });
            this.allPosts = [];
        }
    }

    /**
    *Filter cards based on user input after clicking search icon in search bar.
    */
    searchFilterPostUsingAPI = async (pageCount: number) => {
        this.resetAllFilters();
        if (this.state.searchText.trim().length) {
            let response = await filterTitleAndTags(this.state.searchText, pageCount);

            if (response.status === 200 && response.data) {
                if (!response.data.length) {
                    this.setState({
                        hasMorePosts: false,
                    });
                }
                response.data.map((post: IDiscoverPost) => {
                    let searchedAuthor = this.authorAvatarBackground.find((author) => author.id === post.createdByObjectId);
                    if (searchedAuthor) {
                        post.avatarBackgroundColor = searchedAuthor.color;
                    }
                    else {
                        let color = generateColor();
                        this.authorAvatarBackground.push({ id: post.createdByObjectId, color: color });
                        post.avatarBackgroundColor = color;
                    }

                    if (post.createdByObjectId === this.loggedInUserObjectId) {
                        post.isCurrentUserPost = true;
                    }
                    else {
                        post.isCurrentUserPost = false;
                    }

                    this.allPosts.push(post)
                });

                this.setState({ isPageInitialLoad: false });
                this.getUserVotes();
            }
        }
    }


    /**
    *Filter cards based on 'shared by' checkbox selection.
    *@param selectedCheckboxes User selected checkbox array
    */
    onSharedByCheckboxStateChange = (selectedCheckboxes: Array<ICheckBoxItem>) => {
        this.selectedSharedBy = selectedCheckboxes.filter((value) => { return value.isChecked });
        this.setState({
            isPageInitialLoad: true,
            pageLoadStart: -1,
            infiniteScrollParentKey: this.state.infiniteScrollParentKey + 1,
            discoverPosts: [],
            searchText: "",
            hasMorePosts: true
        });

        this.allPosts = [];
    }

    /**
    *Filter cards based on post type checkbox selection.
    *@param selectedCheckboxes User selected checkbox array
    */
    onTypeCheckboxStateChange = (selectedCheckboxes: Array<ICheckBoxItem>) => {
        this.selectedPostType = selectedCheckboxes.filter((value) => { return value.isChecked });
        this.setState({
            isPageInitialLoad: true,
            pageLoadStart: -1,
            infiniteScrollParentKey: this.state.infiniteScrollParentKey + 1,
            discoverPosts: [],
            searchText: "",
            hasMorePosts: true
        });

        this.allPosts = [];
    }

    /**
    *Filter cards based on tags checkbox selection.
    *@param selectedCheckboxes User selected checkbox array
    */
    onTagsStateChange = (selectedCheckboxes: Array<ICheckBoxItem>) => {
        this.selectedTags = selectedCheckboxes.filter((value) => { return value.isChecked });
        this.setState({
            isPageInitialLoad: true,
            pageLoadStart: -1,
            infiniteScrollParentKey: this.state.infiniteScrollParentKey + 1,
            discoverPosts: [],
            searchText: "",
            hasMorePosts: true
        });

        this.allPosts = [];
    }

    /**
    *Filter cards based sort by value.
    *@param selectedValue Selected value for 'sort by'
    */
    onSortByChange = (selectedValue: string) => {
        this.selectedSortBy = selectedValue;
        this.setState({
            isPageInitialLoad: true,
            pageLoadStart: -1,
            infiniteScrollParentKey: this.state.infiniteScrollParentKey + 1,
            discoverPosts: [],
            searchText: "",
            hasMorePosts: true
        });

        this.allPosts = [];
    }

    /**
    * Invoked when user clicks on vote icon. Updates state and showing notification alert.
    * @param isSuccess Boolean indicating whether edit operation is successful.
    * @param isLiked Boolean indicating whether post is liked or not.
    */
    onVoteClick = (isSuccess: boolean, isLiked: boolean) => {
        if (isSuccess) {
            if (isLiked) {
                this.showAlert(this.localize("voteSuccess"), 1)
            }
            else {
                this.showAlert(this.localize("voteUnliked"), 1)
            }
        }
        else {
            this.showAlert(this.localize("voteError"), 2)
        }
    }

    /**
    * Invoked when post is edited. Updates state and shows notification alert.
    * @param cardDetails Updated post details
    * @param isSuccess Boolean indicating whether edit operation is successful.
    */
    onCardUpdate = (cardDetails: IDiscoverPost, isSuccess: boolean) => {
        if (isSuccess) {
            this.allPosts.map((post: IDiscoverPost) => {
                if (post.ideaId === cardDetails.ideaId) {
                    post.description = cardDetails.description;
                    post.title = cardDetails.title;
                    post.contentUrl = cardDetails.contentUrl;
                    post.tags = cardDetails.tags;
                }
            });

            this.onFilterSearchTextChange(this.filterSearchText);
            this.showAlert(this.localize("postUpdateSuccess"), 1)
        }
        else {
            this.showAlert(this.localize("postUpdateError"), 2)
        }

    }

    /**
    * Invoked when new post is added. Shows notification alert.
    * @param isSuccess Boolean indicating whether add new post operation is successful.
    * @param getSubmittedPost Post details which needs to be added.
    */
    onNewPost = (isSuccess: boolean, getSubmittedPost: IDiscoverPost) => {
        if (isSuccess) {
            let searchedAuthor = this.authorAvatarBackground.find((author) => author.id === getSubmittedPost.createdByObjectId);
            if (searchedAuthor) {
                getSubmittedPost.avatarBackgroundColor = searchedAuthor.color;
            }
            else {
                let color = generateColor();
                this.authorAvatarBackground.push({ id: getSubmittedPost.createdByObjectId, color: color });
                getSubmittedPost.avatarBackgroundColor = color;
            }

            let submittedPost = this.state.discoverPosts;
            if (getSubmittedPost.createdByObjectId === this.loggedInUserObjectId) {
                getSubmittedPost.isCurrentUserPost = true;
            }
            else {
                getSubmittedPost.isCurrentUserPost = false;
            }
            submittedPost.unshift(getSubmittedPost);
            this.setState({ discoverPosts: submittedPost, initialPosts: submittedPost });
            this.allPosts = this.state.discoverPosts;
            this.showAlert(this.localize("addNewPostSuccess"), 1)
        }
        else {
            this.showAlert(this.localize("addNewPostError"), 2)
        }
    }

    /**
    * Filters posts inline by user search text
    * @param searchText Search text entered by user.
    */
    onFilterSearchTextChange = (searchText: string) => {
        this.filterSearchText = searchText;
        if (searchText.trim().length) {
            let filteredPosts = this.allPosts.filter((post: IDiscoverPost) => post.title.toLowerCase().includes(searchText.toLowerCase()) === true);

            this.setState({ discoverPosts: filteredPosts });
        }
        else {
            this.setState({ discoverPosts: [...this.allPosts] });
        }
    }

    /**
    * Invoked when either filter bar is displayed or closed
    * @param isOpen Boolean indicating whether filter bar is displayed or closed.
    */
    handleFilterClear = (isOpen: boolean) => {
        if (!isOpen && (this.selectedPostType.length > 0 || this.selectedSharedBy.length > 0 || this.selectedTags.length > 0 || this.selectedSortBy !== Resources.sortBy[0].id)) {
            this.setState({
                isPageInitialLoad: true,
                pageLoadStart: -1,
                infiniteScrollParentKey: this.state.infiniteScrollParentKey + 1,
                discoverPosts: [],
                searchText: "",
                hasMorePosts: true
            });
            this.allPosts = [];
        }
        this.setState({
            isFilterApplied: isOpen
        });
        this.resetAllFilters();
    }

    /**
    * Invoked when user hits enter or clicks on search icon for searching post through command bar
    */
    invokeApiSearch = () => {
        this.setState({
            isPageInitialLoad: true,
            pageLoadStart: -1,
            infiniteScrollParentKey: this.state.infiniteScrollParentKey + 1,
            discoverPosts: [],
            isFilterApplied: false,
            hasMorePosts: true
        });
        this.allPosts = [];
    }

    hideFilterbar = () => {
        return true;
    }

    /**
     *Get bot settings from API
    */
    async getBotSetting() {
        let response = await getBotSetting(this.teamId!)
        if (response.data) {
            let settings = response.data;
            this.telemetry = settings.instrumentationKey;
            this.botId = settings.botId;
        }
    }

    /**
   *Navigate to submit idea task module.
   */
    handleAddClick = () => {
        let appBaseUrl = window.location.origin;
        microsoftTeams.tasks.startTask({
            completionBotId: this.botId,
            title: this.localize('submitIdeaTaskModuleHeaderText'),
            height: 720,
            width: 700,
            url: `${appBaseUrl}/submit-idea?telemetry=${this.telemetry}`,
            fallbackUrl: `${appBaseUrl}/submit-idea?telemetry=${this.telemetry}`,
        }, this.submitHandler);
    }

    submitHandler = async (err, result) => {
        await this.getDiscoverPosts(0);
    };


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
        if (this.state.loader) {
            return (
                <div className="container-div">
                    <div className="container-subdiv">
                        <div className="loader">
                            <Loader />
                        </div>
                    </div>
                </div>
            );
        } else {

            // Cards component array to be rendered in grid.
            const cards = new Array<any>();

            this.state.discoverPosts!.map((value: IDiscoverPost, index) => 
            {
                 //Ashish: Enhancement for :	The curator app needs an accepted radio button to mark an idea as accepted for development .
                // Added or condition to show cards with Accepted or Approved status.
                if (value.status === ApprovalStatus.Accepted || value.status === ApprovalStatus.Approved) {
                    cards.push(<Col lg={3} sm={6} md={4} className="grid-column d-flex justify-content-center">
                        <Card onAddPrivatePostClick={this.handleUserPrivatePostButtonClick} index={index} cardDetails={value} onVoteClick={this.onVoteClick} onCardUpdate={this.onCardUpdate} onDeleteButtonClick={this.handleDeleteButtonClick} />
                    </Col>)
                }
            });

            if (this.state.initialPosts.length === 0) {
                return (
                    <div className="container-div">
                        <div className="container-subdiv">
                            <NotificationMessage onClose={this.hideAlert} showAlert={this.state.showAlert} content={this.state.alertMessage} notificationType={this.state.alertType} />
                            <NoPostAddedPage showAddButton={true} botId={this.botId} onNewIdeaAdded={this.onNewIdeaAdded} />
                        </div>
                    </div>
                )
            }
            let scrollViewStyle = { height: this.state.isFilterApplied === true ? "84vh" : "92vh" };
            return (
                <div className="container-div">
                    <div className="container-subdiv-cardview">
                        <Container fluid className="container-fluid-overriden">
                            <NotificationMessage
                                onClose={this.hideAlert}
                                showAlert={this.state.showAlert}
                                content={this.state.alertMessage}
                                notificationType={this.state.alertType}
                            />
                            <TitleBar
                                commandBarSearchText={this.state.searchText}
                                searchFilterPostsUsingAPI={this.invokeApiSearch}
                                onFilterClear={this.handleFilterClear}
                                hideFilterbar={!this.state.isFilterApplied}
                                onSortByChange={this.onSortByChange}
                                onFilterSearchChange={this.onFilterSearchTextChange}
                                onSearchInputChange={this.handleSearchInputChange}
                                onSharedByCheckboxStateChange={this.onSharedByCheckboxStateChange}
                                onTypeCheckboxStateChange={this.onTypeCheckboxStateChange}
                                onTagsStateChange={this.onTagsStateChange}
                                showAddNewButton={true}
                                botId={this.botId}
                            />
                            <div key={this.state.infiniteScrollParentKey} className="scroll-view scroll-view-mobile" style={scrollViewStyle}>
                                <InfiniteScroll
                                    pageStart={this.state.pageLoadStart}
                                    loadMore={this.loadMorePosts}
                                    hasMore={this.state.hasMorePosts && !this.filterSearchText.trim().length}
                                    initialLoad={this.state.isPageInitialLoad}
                                    useWindow={false}
                                    loader={<div className="loader"><Loader /></div>}>

                                    <Row>
                                        {
                                            cards.length ? cards : this.state.hasMorePosts === true ? <></> : <FilterNoPostContentPage />
                                        }
                                    </Row>

                                </InfiniteScroll>
                            </div>

                        </Container>
                    </div>
                </div>
            );
        }
    }
}
export default withTranslation()(IdeaWrapperPage)