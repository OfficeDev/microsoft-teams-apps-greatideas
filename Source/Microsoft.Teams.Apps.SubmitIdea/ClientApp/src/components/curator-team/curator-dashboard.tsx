import * as React from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import { Text, Table, ChatIcon, Status, Loader, Flex, List } from '@fluentui/react-northstar';
import { SeverityLevel } from "@microsoft/applicationinsights-web";
import { getApplicationInsightsInstance } from "../../helpers/app-insights";
import { getBotSetting } from "../../api/setting-api";
import { getAllIdeas, filterTitleAndTags } from "../../api/idea-api";
import { createBrowserHistory } from "history";
import CuratorCommandBar from './curator-filter-bar';
import { IdeaEntity, ApprovalStatus } from '../models/idea';
import { Container } from "react-bootstrap";
import UserAvatar from "../curator-team/user-avatar";
import { generateColor } from "../../helpers/helper";
import InfiniteScroll from 'react-infinite-scroller';

import 'bootstrap/dist/css/bootstrap.min.css';
import "../../styles/site.css";

export interface IDashboardState {
    loader: boolean;
    ideas: Array<IdeaEntity>;
    searchText: string;
    infiniteScrollParentKey: number;
    isPageInitialLoad: boolean;
    pageLoadStart: number;
    hasMorePosts: boolean;
    open: boolean;
    initialPosts: Array<IdeaEntity>;
    screenWidth: number;
}

const browserHistory = createBrowserHistory({ basename: "" });

class CuratorTeamDashboard extends React.Component<WithTranslation, IDashboardState> {
    localize: TFunction
    telemetry?: string = "";
    teamId?: string | null;
    userObjectId?: string = "";
    appInsights: any;
    appBaseUrl: string = window.location.origin;
    botId: string = "";
    allPosts: Array<IdeaEntity>;
    selectedSortBy: string;
    filterSearchText: string;
    hasmorePost: boolean;

    constructor(props: any) {
        super(props);
        this.localize = this.props.t;
        this.allPosts = [];
        this.selectedSortBy = "";
        this.filterSearchText = "";
        this.telemetry = "";
        this.hasmorePost = true;

        this.state = {
            loader: true,
            searchText: "",
            infiniteScrollParentKey: 0,
            isPageInitialLoad: true,
            pageLoadStart: -1,
            hasMorePosts: true,
            open: false,
            ideas: [],
            initialPosts: [],
            screenWidth: 0
        }
    }

    public componentDidMount() {
        microsoftTeams.initialize();
        microsoftTeams.getContext((context) => {
            this.userObjectId = context.userObjectId;

            // Initialize application insights for logging events and errors.
            this.appInsights = getApplicationInsightsInstance(this.telemetry, browserHistory);
            this.teamId = context.teamId;
            this.getBotSetting();
            this.initIdeas();
            this.update();
        });
    }

    /**
    * get screen width real time
    */
    update = () => {
        if (window.innerWidth !== this.state.screenWidth) {
            this.setState({ screenWidth: window.innerWidth });
        }
    };

    /**
    * Fetch posts for initializing grid
    */
    initIdeas = async () => {
        this.appInsights.trackTrace({ message: `'getIdeas' - Initiated request`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
        let response = await getAllIdeas(0);

        if (response.status === 200 && response.data) {
            this.appInsights.trackTrace({ message: `'getIdeas' - Request success`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
            this.setState({
                initialPosts: response.data,
                loader: false
            });

        }
        else {
            this.appInsights.trackTrace({ message: `'getIdeas' - Request failed`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
        }
        this.setState({
            loader: false
        });
    }

    onChatButtonClick = (ideaDetails: IdeaEntity) => {
        let msg = this.localize('chatWithIdeator', { ideator: ideaDetails.createdByName, idea: ideaDetails.title });
        let url = `https://teams.microsoft.com/l/chat/0/0?users=${encodeURIComponent(ideaDetails.createdByUserPrincipalName!)}&message=${encodeURIComponent(msg)}`;
        microsoftTeams.executeDeepLink(url);
    }

    /**
    *Get ideas from API
    */
    async getIdeas(pageCount: number) {
        let response = await getAllIdeas(pageCount);

        if (response.status === 200 && response.data) {
            if (response.data.length < 50) {
                this.hasmorePost = false;
            }

            var posts = response.data.map((idea: IdeaEntity) => {
                let color = generateColor();
                idea.backgroundColor = color;
                return idea;
            });

            this.allPosts = [...this.allPosts, ...posts];

            this.setState({
                ideas: [...this.allPosts], loader: false, hasMorePosts: this.hasmorePost, isPageInitialLoad: false
            });
        }

        this.setState({
            loader: false
        });
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
    *Navigate to manage category task module.
    */
    onManageCategoryButtonClick = () => {
        microsoftTeams.tasks.startTask({
            completionBotId: this.botId,
            title: this.localize('manageCategoryTitle'),
            height: 600,
            width: 900,
            url: `${this.appBaseUrl}/manage-category?telemetry=${this.telemetry}`,
            fallbackUrl: `${this.appBaseUrl}/manage-category?telemetry=${this.telemetry}`
        });
    }

    handleItemClick = (createdByObjectId: string, ideaId: string) => {
        microsoftTeams.tasks.startTask({
            completionBotId: this.botId,
            title: this.localize('viewIdeaTitle'),
            height: 660,
            width: 900,
            url: `${this.appBaseUrl}/view-idea?telemetry=${this.telemetry}&id=${ideaId}&userId=${createdByObjectId}`,
            fallbackUrl: `${this.appBaseUrl}/view-idea?telemetry=${this.telemetry}&id=${ideaId}&userId=${createdByObjectId}`,
        }, this.submitHandler);
    }

    submitHandler = async (err, result) => {

        this.setState({
            isPageInitialLoad: true,
            pageLoadStart: -1,
            ideas: [],
            infiniteScrollParentKey: this.state.infiniteScrollParentKey + 1,
            hasMorePosts: true
        });

        this.hasmorePost = true;
        this.allPosts = [];
    };

    /**
    *Invoked by Infinite scroll component when user scrolls down to fetch next set of posts
    *@param pageCount Page count for which next set of ideas needs to be fetched
    */
    loadMorePosts = (pageCount: number) => {
        if (this.state.searchText.trim().length) {
            this.searchFilterPostUsingAPI(pageCount);
        } else {
            this.getIdeas(pageCount);
        }
    }

    /**
    *Filter cards based on user input after clicking search icon in search bar.
    */
    searchFilterPostUsingAPI = async (pageCount: number) => {
        if (this.state.searchText.trim().length) {
            let response = await filterTitleAndTags(this.state.searchText, pageCount);
            if (response.status === 200 && response.data) {
                if (response.data.length < 50) {
                    this.hasmorePost = false;
                }
                var posts = response.data.map((idea: IdeaEntity) => {
                    let color = generateColor();
                    idea.backgroundColor = color;
                    return idea;
                });

                this.allPosts = [...this.allPosts, ...posts];

                this.setState({
                    ideas: [...this.allPosts], loader: false, hasMorePosts: this.hasmorePost, isPageInitialLoad: false
                });
            }
        }
    }

    getApprovalStatus = (type: number | undefined) => {
        if (type === ApprovalStatus.Pending) {
            return this.localize('pendingStatusText');
        }
        else if (type === ApprovalStatus.Approved) {
            return this.localize('approvedStatusText');
        }
        else if (type === ApprovalStatus.Rejected) {
            return this.localize('rejectedStatusText');
        }
        else {
            return this.localize('pendingStatusText');
        }
    }

    /**
    * Invoked when user hits enter or clicks on search icon for searching post through command bar
    */
    invokeApiSearch = () => {
        this.setState({
            isPageInitialLoad: true,
            pageLoadStart: -1,
            ideas: [],
            infiniteScrollParentKey: this.state.infiniteScrollParentKey + 1,
            hasMorePosts: true
        });

        this.hasmorePost = true;
        this.allPosts = [];
    }

    /**
    *Set state of search text as per user input change
    *@param searchText Search text entered by user
    */
    handleSearchInputChange = (searchText: string) => {
        this.setState({
            searchText: searchText,
        });

        if (searchText.length === 0) {
            this.setState({
                isPageInitialLoad: true,
                pageLoadStart: -1,
                infiniteScrollParentKey: this.state.infiniteScrollParentKey + 1,
                ideas: [],
                hasMorePosts: true
            });

            this.hasmorePost = true;
            this.allPosts = [];
        }
    }

    async getSearchResults(searchText: string) {
        this.setState({
            loader: true
        });
        let response = await filterTitleAndTags(searchText, this.state.infiniteScrollParentKey!);

        if (response.status === 200 && response.data) {
            this.appInsights.trackTrace({ message: `'getIdeas' - Request success`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });

            this.setState({
                ideas: response.data,
            });
        }
        else {
            this.appInsights.trackTrace({ message: `'getIdeas' - Request failed`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
        }
        this.setState({
            loader: false
        });
    }

    /**
    *Returns text component containing error message for failed name field validation
    *@param {boolean} isValuePresent Indicates whether value is present
    */
    private getRequiredFieldError = (isValuePresent: boolean) => {
        if (!isValuePresent) {
            return (<Text content={this.localize('fieldRequiredMessage')} className="field-error-message" error size="medium" />);
        }

        return (<></>);
    }


    renderIdeas = () => {

        const ideasTableHeader = {
            key: "header",
            items:
                [
                    { content: <Text weight="semibold" content={this.localize('ideaName')} />, key: "name", className: "margin-left-medium" },
                    { content: <Text weight="semibold" content={this.localize('category')} />, key: "category", className: "category-cell" },
                    { content: <Text weight="semibold" content={this.localize('ideatorName')} />, key: "ideatorName", className: "name-cell" },
                    { content: <Text weight="semibold" content={this.localize('status')} />, key: "status", className: "status-cell" },
                    { content: <Text content="" />, key: "chat", className: "chat-cell" }
                ]
        };


        let ideasTableRows = this.state.ideas ?.map((value, index) => (
            {
                key: index,
                style: {},
                items:
                    [
                        { content: <Text content={value.title} title={value.title} />, key: index + "1", truncateContent: true, onClick: () => this.handleItemClick(value.createdByObjectId!, value.ideaId!), className: "hover-effect margin-left-medium" },
                        { content: <><Status color={value.backgroundColor} />&nbsp;&nbsp;<Text content={value.category} title={value.category} /></>, key: index + "2", truncateContent: true, className: "category-cell" },
                        {
                            content: <UserAvatar avatarColor={value.backgroundColor!} showFullName={true} postType={value.category!} content={value.createdByName!} title={value.createdByName!} />, key: index + "3", truncateContent: true, className: "name-cell"
                        },
                        { content: <Text content={this.getApprovalStatus(value.status)} title={this.getApprovalStatus(value.status)} />, key: index + "4", truncateContent: true, className: "status-cell" },
                        { content: <ChatIcon outline onClick={() => this.onChatButtonClick(value)} title={this.localize("chatWithText", { ideator: value.createdByName })} />, key: index + "5", truncateContent: true, className: "chat-cell hover-effect" }
                    ]
            }
        ));

        let privateListListViewItems = this.state.ideas ?.map((value, index: number) => (
            {
                key: index,
                header: <></>,
                contentMedia: <></>,
                content:
                    <Flex vAlign="stretch">
                        <Flex.Item>
                            <Flex column gap="gap.small" vAlign="stretch">
                                <Flex>
                                    <Text weight="bold" className="content-text-private-list" onClick={() => this.handleItemClick(value.createdByObjectId!, value.ideaId!)} content={value.title} title={value.title} />
                                </Flex>
                                <Flex>
                                    <><Status className="subtitle-margin" color={value.backgroundColor!} />&nbsp;&nbsp;<Text className="content-text-private-list" content={value.category} title={value.category} /></>
                                </Flex>
                                <Flex>
                                    <UserAvatar avatarColor={value.backgroundColor!} showFullName={true} postType={value.category!} content={value.createdByName!} title={value.createdByName!} />
                                </Flex>
                                <Flex>
                                    <Text content={this.getApprovalStatus(value.status)} title={this.getApprovalStatus(value.status)} />
                                </Flex>
                                <Flex className="margin-bottom-list">
                                    <ChatIcon outline onClick={() => this.onChatButtonClick(value)} title={this.localize("chatWithText", { ideator: value.createdByName })} />
                                </Flex>
                            </Flex>
                        </Flex.Item>
                    </Flex>,
                className: "list-item"
            }
        ));

        return (
            <div key={this.state.infiniteScrollParentKey} className="scroll-view scroll-view-mobile" style={{ height: "92vh" }}>
                <InfiniteScroll
                    pageStart={this.state.pageLoadStart}
                    loadMore={this.loadMorePosts}
                    hasMore={this.state.hasMorePosts && !this.filterSearchText.trim().length}
                    initialLoad={this.state.isPageInitialLoad}
                    useWindow={false}
                    loader={<div className="loader"><Loader /></div>}>

                    {this.state.screenWidth <= 750 && <List items={privateListListViewItems} />}
                    {this.state.screenWidth > 750 && <Table rows={ideasTableRows} header={ideasTableHeader} />}
                </InfiniteScroll>
            </div>
        );
    }

    public render(): JSX.Element {
        return (
            <div className="container-div">
                <div className="container-subdiv-cardview">
                    <Container className="container-fluid-overriden" fluid>
                        <CuratorCommandBar
                            onManageCategoryButtonClick={this.onManageCategoryButtonClick}
                            onSearchInputChange={this.handleSearchInputChange}
                            searchFilterPostsUsingAPI={this.invokeApiSearch}
                            commandBarSearchText={this.state.searchText}
                        />
                        <div className="margin-top-medium">
                            {this.renderIdeas()}
                        </div>
                    </Container>
                </div>
            </div>
        );
    }
}

export default withTranslation()(CuratorTeamDashboard)