// <copyright file="upvote-idea-dialog.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Button, Flex, Text, ItemLayout, Image, Provider, Label, Loader } from "@fluentui/react-northstar";
import { CloseIcon } from "@fluentui/react-icons-northstar";
import * as microsoftTeams from "@microsoft/teams-js";
import { IDiscoverPost } from "../card-view/idea-wrapper-page";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import { IdeaEntity } from "../models/idea";
import UserAvatar from "../curator-team/user-avatar";
import { generateColor } from "../../helpers/helper";
import Constants from "../../constants/resources";
import { getIdea } from "../../api/idea-api";
import "../../styles/edit-dialog.css";
import "../../styles/card.css";

let moment = require('moment');

interface IIdeaDialogContentProps extends WithTranslation {
    cardDetails: IDiscoverPost
    onVoteClick: () => void;
    changeDialogOpenState: (isOpen: boolean) => void;
}

interface IIdeaDialogContentState {
    idea: IdeaEntity | undefined,
    loading: boolean,
    submitLoading: boolean,
    isEditDialogOpen: boolean,
    theme: string;
}

class UpvoteIdeaDialogContent extends React.Component<IIdeaDialogContentProps, IIdeaDialogContentState> {
    localize: TFunction;
    teamId = "";
    constructor(props: any) {
        super(props);

        this.localize = this.props.t;
        this.state = {
            loading: true,
            idea: undefined,
            submitLoading: false,
            isEditDialogOpen: false,
            theme: ""
        }
    }

    componentDidMount() {
        microsoftTeams.initialize();
        microsoftTeams.getContext((context: microsoftTeams.Context) => {
            this.teamId = context.teamId!;
            this.setState({ theme: context.theme! });
            this.getIdea(this.props.cardDetails?.createdByObjectId!, this.props.cardDetails?.ideaId);
        });
    }

    /**
   *Get idea details from API
   */
    async getIdea(createdById: string, ideaId: string) {
        let response = await getIdea(createdById!, ideaId!);
        if (response.status === 200 && response.data) {

            let idea = response.data as IdeaEntity;
            let color = generateColor();
            idea.backgroundColor = color;

            this.setState({ idea: idea });
        }
        this.setState({
            loading: false
        });
    }

	/**
	*Close the dialog and pass back card properties to parent component.
	*/
    onSubmitClick = async () => {
        this.props.onVoteClick();
        this.props.changeDialogOpenState(false);
    }


	/**
	* Renders the component
	*/
    public render(): JSX.Element {
        if (!this.state.loading) {
            return (
                <Provider className="dialog-provider-wrapper">
                    <Flex>
                        <Flex.Item grow>
                            <ItemLayout
                                className="app-name-container"
                                media={<Image className="app-logo-container" src="/Artifacts/applicationLogo.png" />}
                                header={<Text content={this.localize("dialogTitleAppName")} weight="bold" />}
                                content={<Text content={this.localize("viewIdeaTitle")} weight="semibold" size="small" />}
                            />
                        </Flex.Item>
                        <CloseIcon className="icon-hover close-icon" onClick={() => this.props.changeDialogOpenState(false)} />
                    </Flex>
                    <Flex>
                        <div className="dialog-body">
                            {this.state.idea && <Flex column gap="gap.small">
                                <Flex vAlign="center" space="between">
                                    <Flex.Item>
                                        <Flex gap="gap.small" column>
                                            <Text size="largest" className="word-break" weight="bold" content={this.state.idea.title} />
                                            <Flex wrap className="margin-subcontent" vAlign="center"><UserAvatar avatarColor={generateColor()}
                                                showFullName={true} postType={this.state.idea.category!}
                                                content={this.state.idea.createdByName!} title={this.state.idea.title!} />
                                    &nbsp;<Text content={this.localize("ideaPostedOnText", { time: moment(new Date(this.state.idea.createdDate!)).format("llll") })} />
                                            </Flex>
                                        </Flex>
                                    </Flex.Item>
                                    <Flex.Item align="end" push>
                                        <Flex column>
                                            <Text size="largest" weight="bold" content={this.state.idea.totalVotes} />
                                            <Text size="small" content={this.localize("UpvotesText")} />
                                        </Flex>
                                    </Flex.Item>
                                </Flex>
                                <Flex column>
                                    <Text content={this.localize("SynopsisText")} weight="bold" />
                                    <Text className="word-break" content={this.state.idea.description} />
                                </Flex>
                                <Flex column>
                                    <Text content={this.localize("supportingDocumentsTitle")} weight="bold" />
                                    <div className="documents-area document-text">
                                        {this.state.idea.documentLinks && JSON.parse(this.state.idea.documentLinks).map((document, index) => <Flex key={index} gap="gap.smaller"><Text className="document-hover" truncated content={document} onClick={() => window.open(document, "_blank")} /></Flex>)}
                                    </div>
                                </Flex>
                                <Flex>
                                    <Flex.Item size="size.half">
                                        <Flex column >
                                            <Text content={this.localize("tagsTitle")} weight="bold" />
                                            <div>
                                                {this.state.idea.tags && this.state.idea.tags?.split(";")?.map((tag, index) => <Label className={this.state.theme === Constants.dark ? "tags-label-wrapper-dark" : "tags-label-wrapper"} circular
                                                    key={index} content={tag} />)}
                                            </div>
                                        </Flex>
                                    </Flex.Item>
                                    <Flex.Item size="size.half">
                                        <Flex column gap="gap.smaller">
                                            <Text weight="bold" content={this.localize("category")} />
                                            <Text content={this.state.idea.category} />
                                        </Flex>
                                    </Flex.Item>
                                </Flex>

                            </Flex>}
                        </div>
                    </Flex>
                    <Flex className="dialog-footer-wrapper">
                        <Flex gap="gap.smaller" className="dialog-footer input-fields-margin-between-add-post">
                            <div></div>
                            <Flex.Item push>
                                <Button content={this.props.cardDetails.isVotedByUser === true ? this.localize("unlikeButtonText") : this.localize("UpvoteButtonText")}
                                    primary loading={this.state.submitLoading} disabled={this.state.submitLoading} onClick={this.onSubmitClick} />
                            </Flex.Item>

                        </Flex>
                    </Flex>
                </Provider>
            );
        }
        else {
            return <Loader />
        }
    }
}

export default withTranslation()(UpvoteIdeaDialogContent)