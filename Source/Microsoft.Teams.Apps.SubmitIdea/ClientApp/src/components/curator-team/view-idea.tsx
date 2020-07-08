// <copyright file="view-idea.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { WithTranslation, withTranslation } from "react-i18next";
import * as microsoftTeams from "@microsoft/teams-js";
import { Text, Flex, Provider, Label, RadioGroup, TextArea, Loader, Image, Button, Dropdown } from "@fluentui/react-northstar";
import { TFunction } from "i18next";
import { IdeaEntity, ApprovalStatus } from "../models/idea";
import UserAvatar from "./user-avatar";
import { generateColor, isNullorWhiteSpace } from "../../helpers/helper";
import { ICategoryDetails } from "../models/category";
import { getAllCategories } from "../../api/category-api";
import { getIdea, updatePostContent } from "../../api/idea-api";
import { SeverityLevel } from "@microsoft/applicationinsights-web";
import { getApplicationInsightsInstance } from "../../helpers/app-insights";
import Constants from "../../constants/resources";
import { createBrowserHistory } from "history";
let moment = require('moment');

interface IState {
    idea: IdeaEntity | undefined,
    loading: boolean,
    theme: string;
    selectedStatus: number | undefined,
    selectedCategory: string | undefined,
    feedbackText: string | undefined,
    categories: Array<ICategoryDetails>,
    submitLoading: boolean,
    isCategorySelected: boolean,
    feedbackTextEmpty: boolean,
    isIdeaApprovedOrRejected: boolean;
}

const browserHistory = createBrowserHistory({ basename: "" });

class ViewIdea extends React.Component<WithTranslation, IState> {
    localize: TFunction;
    userObjectId: string | undefined = "";
    items: any;
    appInsights: any;
    telemetry: string | undefined = "";
    ideaId: string | undefined = "";
    createdById: string | undefined = "";
    appUrl: string = (new URL(window.location.href)).origin;

    constructor(props) {
        super(props);
        this.localize = this.props.t;
        this.state = {
            loading: true,
            idea: undefined,
            selectedStatus: ApprovalStatus.Approved,
            selectedCategory: undefined,
            categories: [],
            feedbackText: "",
            theme: "",
            submitLoading: false,
            isCategorySelected: false,
            feedbackTextEmpty: true,
            isIdeaApprovedOrRejected: false,
        }
        this.items = [
            {
                key: 'approve',
                label: this.localize('radioApprove'),
                value: ApprovalStatus.Approved,
            },
            {
                key: 'reject',
                label: this.localize('radioReject'),
                value: ApprovalStatus.Rejected,
            }
        ]

        let params = new URLSearchParams(window.location.search);
        this.telemetry = params.get("telemetry")!;
        this.ideaId = params.get("id")!;
        this.createdById = params.get("userId")!;
    }



    public componentDidMount() {
        microsoftTeams.initialize();
        microsoftTeams.getContext((context) => {
            this.userObjectId = context.userObjectId!;
            this.setState({ theme: context.theme! });

            // Initialize application insights for logging events and errors.
            this.appInsights = getApplicationInsightsInstance(this.telemetry, browserHistory);
            this.getCategory();
        });
    }

    getA11SelectionMessage = {
        onAdd: item => {
            if (item) { this.setState({ selectedCategory: item, isCategorySelected: true }) };
            return "";
        },
    };

    /**
     *Get idea details from API
    */
    async getIdea() {
        this.appInsights.trackTrace({ message: `'getIdea' - Initiated request`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
        let response = await getIdea(this.createdById!, this.ideaId!);
        if (response.status === 200 && response.data) {
            this.appInsights.trackTrace({ message: `'getIdea' - Request success`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });

            let idea = response.data as IdeaEntity;
            let category = this.state.categories.filter(row => row.categoryName === idea.category).shift();
            if (category === undefined) {
                this.setState({ selectedCategory: undefined });
            }
            else {
                this.setState({ selectedCategory: idea.category, isCategorySelected: true });
            }

            let color = generateColor();
            idea.backgroundColor = color;
            this.setState(
                {
                    loading: false,
                    idea: idea,
                });
        }
        else {
            this.appInsights.trackTrace({ message: `'getIdea' - Request failed`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
        }
        this.setState({
            loading: false
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
            this.setState({
                categories: category.data,
            });

            await this.getIdea();
        }
        else {
            this.appInsights.trackTrace({ message: `'getCategory' - Request failed`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
        }
        this.setState({
            loading: false
        });
    }

    /**
    *Approve or rejectIdea
    */
    async approveOrRejectIdea(idea: any) {
        this.appInsights.trackTrace({ message: `'approveOrRejectIdea' - Initiated request`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
        let updateEntity = await updatePostContent(idea);

        if (updateEntity.status === 200 && updateEntity.data) {
            this.appInsights.trackTrace({ message: `'approveOrRejectIdea' - Request success`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
        }
        else {
            this.appInsights.trackTrace({ message: `'approveOrRejectIdea' - Request failed`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
        }

        this.setState({
            loading: false,
            submitLoading: false,
            isIdeaApprovedOrRejected: true,
        });
    }

    /**
   * Handle radio group change event.
   * @param e | event
   * @param props | props
   */
    handleChange = (e: any, props: any) => {
        this.setState({ selectedStatus: props.value })
    }

    checkIfConfirmAllowed = () => {
        if (this.state.selectedCategory === undefined) {
            this.setState({ isCategorySelected: false });
            return false;
        }

        if (this.state.selectedStatus === 2 && isNullorWhiteSpace(this.state.feedbackText!)) {
            this.setState({ feedbackTextEmpty: false });
            return false;
        }

        return true;
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

    handleConfirm = () => {
        if (this.checkIfConfirmAllowed()) {
            this.setState({ submitLoading: true });
            let category = this.state.categories.filter(row => row.categoryName === this.state.selectedCategory).shift();
            let updateEntity: IdeaEntity = {
                ideaId: this.state.idea?.ideaId,
                feedback: this.state.selectedStatus === ApprovalStatus.Rejected ? this.state.feedbackText : "",
                status: this.state.selectedStatus,
                category: this.state.selectedCategory,
                categoryId: category?.categoryId,
                approverOrRejecterUserId: this.userObjectId,
                createdByObjectId: this.state.idea?.createdByObjectId,
                title: this.state.idea?.title,
                description: this.state.idea?.description,
                documentLinks: this.state.idea?.documentLinks,
                totalVotes: this.state.idea?.totalVotes,
                tags: this.state.idea?.tags,
                createdDate: this.state.idea?.createdDate,
                createdByName: this.state.idea?.createdByName,
                createdByUserPrincipalName: this.state.idea?.createdByUserPrincipalName,
                updatedDate: this.state.idea?.updatedDate,
                approvedOrRejectedByName: this.state.idea?.approvedOrRejectedByName
            }

            this.approveOrRejectIdea(updateEntity);
        }
    }

    onFeedbackChange = (value: string) => {
        this.setState({ feedbackText: value });
    }

    /**
     * Renders the component.
    */
    public render(): JSX.Element {
        if (!this.state.loading && !this.state.isIdeaApprovedOrRejected) {
            return (
                <Provider>
                    <div className="module-container">
                        <div className="tab-container">
                            {this.state.idea && <Flex column>
                                <Flex className="top-margin">
                                    <Text size="largest" className="word-break" weight="bold" content={this.state.idea.title} />
                                </Flex>
                                <Flex wrap className="subtitle-margin" vAlign="center">
                                    <UserAvatar avatarColor={this.state.idea.backgroundColor!} showFullName={true}
                                        postType={this.state.idea.category!} content={this.state.idea.createdByName!}
                                        title={this.state.idea.createdByName!} />
                                &nbsp;<Text content={this.localize("ideaPostedOnText", { time: moment(new Date(this.state.idea.createdDate!)).format("llll") })} /></Flex>
                                <Flex className="add-toppadding">
                                    <Flex.Item>
                                        <Text content={this.localize("synopsisTitle")} weight="bold" />
                                    </Flex.Item>
                                </Flex>
                                <Flex>
                                    <Flex.Item>
                                        <div>
                                            <Text className="word-break" content={this.state.idea.description} />
                                        </div>
                                    </Flex.Item>
                                </Flex>
                                <Flex wrap className="add-toppadding">
                                    <Flex.Item size="size.half">
                                        <Flex column>
                                            <Text content={this.localize("tagsTitle")} weight="bold" />
                                            <div className="margin-top-small">
                                                {this.state.idea.tags && this.state.idea.tags?.split(";").map((tag, index) => <Label circular className={this.state.theme === Constants.dark ? "tags-label-wrapper-dark" : "tags-label-wrapper"} key={index} content={tag} />)}
                                            </div>
                                            <Text className="add-toppadding" content={this.localize("supportingDocumentsTitle")} weight="bold" />
                                            <div className="documents-area document-width"><div className="document-text">{this.state.idea.documentLinks && JSON.parse(this.state.idea.documentLinks).map((document) => <Flex ><Text className="document-hover" truncated content={document} onClick={() => window.open(document, "_blank")} /></Flex>)}</div></div>
                                        </Flex>
                                    </Flex.Item>
                                    <Flex.Item size="size.half" className="add-toppadding" >
                                        <Flex column gap="gap.small">
                                            <Text content={this.localize("category")} weight="bold" />
                                            <Flex.Item push>
                                                {this.getRequiredFieldError(this.state.isCategorySelected)}
                                            </Flex.Item>
                                            <Flex.Item>
                                                <Dropdown fluid className="category-length"
                                                    items={this.state.categories.map((category) => category.categoryName)}
                                                    value={this.state.selectedCategory}
                                                    placeholder={this.localize("categoryPlaceholder")}
                                                    getA11ySelectionMessage={this.getA11SelectionMessage}
                                                    disabled={this.state.idea.status !== ApprovalStatus.Pending}
                                                />
                                            </Flex.Item>
                                            {this.state.idea.status === ApprovalStatus.Pending && <><Text content={this.localize("confirmation")} weight="bold" />
                                                <RadioGroup items={this.items}
                                                    defaultCheckedValue={this.state.selectedStatus}
                                                    onCheckedValueChange={this.handleChange}
                                                /></>}
                                            {this.state.selectedStatus === ApprovalStatus.Rejected && <>
                                                {this.getRequiredFieldError(this.state.feedbackTextEmpty)}
                                                <TextArea className="reason-text-area" fluid maxLength={150} placeholder={this.localize("reasonForRejectionText")}
                                                    value={this.state.feedbackText} onChange={(event: any) => this.onFeedbackChange(event.target.value)} /></>}

                                        </Flex>
                                    </Flex.Item>
                                </Flex>
                            </Flex>}

                        </div>
                        <div className="tab-footer">
                            {this.state.idea?.status === ApprovalStatus.Pending && <Flex hAlign="end" ><Button primary disabled={this.state.submitLoading} loading={this.state.submitLoading}
                                content={this.localize("Confirm")} onClick={this.handleConfirm} /></Flex>}
                        </div>
                    </div>
                </Provider>)
        }
        else if (this.state.isIdeaApprovedOrRejected) {
            return (
                <div className="submit-idea-success-message-container">
                    <Flex column gap="gap.small">
                        <Flex hAlign="center" className="margin-space">{this.state.selectedStatus === ApprovalStatus.Approved ? <Image className="preview-image-icon" fluid src={this.appUrl + "/Artifacts/successIcon.png"} /> : <Image className="preview-image-icon" fluid src={this.appUrl + "/Artifacts/rejectIcon.png"} />} </Flex>
                        <Flex hAlign="center" className="space">
                            <Text weight="bold"
                                content={this.state.selectedStatus === ApprovalStatus.Approved ? this.localize("approvedIdeaSuccessMessage") : this.localize("rejectedIdeaMessage")}
                                size="medium"
                            />
                        </Flex>
                    </Flex>
                </div>)
        }
        else {
            return <Loader />
        }
    }
}

export default withTranslation()(ViewIdea)