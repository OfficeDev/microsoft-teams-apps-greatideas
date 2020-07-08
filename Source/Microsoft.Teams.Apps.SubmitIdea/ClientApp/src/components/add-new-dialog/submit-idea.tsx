// <copyright file="submit-idea.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { WithTranslation, withTranslation } from "react-i18next";
import * as microsoftTeams from "@microsoft/teams-js";
import { Text, Flex, Provider, Label, Input, AddIcon, Loader, Image, Button, CloseIcon, InfoIcon, TextArea, Dropdown, TrashCanIcon } from "@fluentui/react-northstar";
import { TFunction } from "i18next";
import { IdeaEntity, ApprovalStatus } from "../models/idea";
import { isNullorWhiteSpace } from "../../helpers/helper";
import { ICategoryDetails } from "../models/category";
import { getAllCategories } from "../../api/category-api";
import { addNewPostContent } from "../../api/idea-api";
import { createBrowserHistory } from "history";
import Constants from "../../constants/resources";

import "../../styles/submit-idea.css";


interface IState {
    loading: boolean,
    selectedCategory: string | undefined,
    categories: Array<ICategoryDetails>,
    theme: string;
    tagValidation: ITagValidationParameters;
    tagsList: Array<string>;
    tag: string;
    documentsList: Array<string>;
    documentLink: string;
    isTitlePresent: boolean,
    isDescriptionPresent: boolean,
    isCategorySelected: boolean,
    isLinkValid: boolean,
    submitLoading: boolean,
    ideaTitle: string,
    ideaDescription: string,
    alertMessage: string,
    alertType: number,
    showAlert: boolean,
    isIdeaSubmitedsuccessfully: boolean,
}

export interface ITagValidationParameters {
    isEmpty: boolean;
    isExisting: boolean;
    isLengthValid: boolean;
    isTagsCountValid: boolean;
    containsSemicolon: boolean;
}

const browserHistory = createBrowserHistory({ basename: "" });

class SubmitIdea extends React.Component<WithTranslation, IState> {
    localize: TFunction;
    userObjectId: string = "";
    upn: string = "";
    items: any;
    appUrl: string = (new URL(window.location.href)).origin;

    constructor(props) {
        super(props);
        this.localize = this.props.t;
        this.state = {
            loading: true,
            selectedCategory: undefined,
            categories: [],
            theme: "",
            isTitlePresent: true,
            isDescriptionPresent: true,
            isCategorySelected: true,
            isLinkValid: true,
            ideaTitle: "",
            ideaDescription: "",
            submitLoading: false,
            documentLink: "",
            documentsList: [],
            tagsList: [],
            tag: "",
            tagValidation: { isEmpty: false, isExisting: false, isLengthValid: true, isTagsCountValid: true, containsSemicolon: false },
            alertMessage: "",
            alertType: 0,
            showAlert: false,
            isIdeaSubmitedsuccessfully: false
        }
    }

    public componentDidMount() {
        microsoftTeams.initialize();
        microsoftTeams.getContext((context) => {
            this.userObjectId = context.userObjectId!;
            this.upn = context.upn!;
            this.setState({ theme: context.theme! });
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
  *Get categories from API
  */
    async getCategory() {
        let category = await getAllCategories();

        if (category.status === 200 && category.data) {

            this.setState({
                categories: category.data,
            });
        }
        else {
        }
        this.setState({
            loading: false
        });
    }

    checkIfSubmitAllowed = () => {
        if (isNullorWhiteSpace(this.state.ideaTitle)) {
            this.setState({ isTitlePresent: false });
            return false;
        }

        if (isNullorWhiteSpace(this.state.ideaDescription)) {
            this.setState({ isDescriptionPresent: false });
            return false;
        }
        if (this.state.selectedCategory === undefined) {
            this.setState({ isCategorySelected: false });
            return false;
        }

        return true;
    }

    /**
	*Check if tag is valid
	*/
    checkIfTagIsValid = () => {
        let validationParams: ITagValidationParameters = { isEmpty: false, isLengthValid: true, isExisting: false, isTagsCountValid: false, containsSemicolon: false };
        if (this.state.tag.trim() === "") {
            validationParams.isEmpty = true;
        }

        if (this.state.tag.length > Constants.tagMaxLength) {
            validationParams.isLengthValid = false;
        }

        let tags = this.state.tagsList;
        let isTagExist = tags.find((tag: string) => {
            if (tag.toLowerCase() === this.state.tag.toLowerCase()) {
                return tag;
            }
        });

        if (this.state.tag.split(";").length > 1) {
            validationParams.containsSemicolon = true;
        }

        if (isTagExist) {
            validationParams.isExisting = true;
        }

        if (this.state.tagsList.length < Constants.tagsMaxCount) {
            validationParams.isTagsCountValid = true;
        }

        this.setState({ tagValidation: validationParams });

        if (!validationParams.isEmpty && !validationParams.isExisting && validationParams.isLengthValid && validationParams.isTagsCountValid && !validationParams.containsSemicolon) {
            return true;
        }
        return false;
    }

    /**
	*Sets state of tagsList by removing tag using its index.
	*@param index Index of tag to be deleted.
	*/
    onTagRemoveClick = (index: number) => {
        let tags = this.state.tagsList;
        tags.splice(index, 1);
        this.setState({ tagsList: tags });
    }

    /**
    *Returns text component containing error message for empty tag input field
    */
    private getTagError = () => {
        if (this.state.tagValidation.isEmpty) {
            return (<Text content={this.localize("emptyTagError")} className="field-error-message" error size="medium" />);
        }
        else if (!this.state.tagValidation.isLengthValid) {
            return (<Text content={this.localize("tagLengthError")} className="field-error-message" error size="medium" />);
        }
        else if (this.state.tagValidation.isExisting) {
            return (<Text content={this.localize("sameTagExistsError")} className="field-error-message" error size="medium" />);
        }
        else if (!this.state.tagValidation.isTagsCountValid) {
            return (<Text content={this.localize("tagsCountError")} className="field-error-message" error size="medium" />);
        }
        else if (this.state.tagValidation.containsSemicolon) {
            return (<Text content={this.localize("semicolonTagError")} className="field-error-message" error size="medium" />);
        }
        return (<></>);
    }

    /**
	*Sets state of tagsList by adding new tag.
	*/
    onTagAddClick = () => {
        if (this.checkIfTagIsValid()) {
            let tagList = this.state.tagsList;
            tagList.push(this.state.tag.toLowerCase());
            this.setState({ tagsList: tagList, tag: "" });
        }
    }

    /**
	* Adds tag when enter key is pressed
	* @param event Object containing event details
	*/
    onTagKeyDown = (event: any) => {
        if (event.key === 'Enter') {
            this.onTagAddClick();
        }
    }


	/**
	*Sets tag state.
	*@param tag Tag string
	*/
    onTagChange = (tag: string) => {
        this.setState({ tag: tag })
    }

    /**
	*Sets title state.
	*@param title Title string
	*/
    onTitleChange = (value: string) => {
        this.setState({ ideaTitle: value, isTitlePresent: true });
    }

    /**
	*Sets description state.
	*@param description Description string
	*/
    onDescriptionChange = (description: string) => {
        this.setState({ ideaDescription: description, isDescriptionPresent: true });
    }

    /**
	*Sets document link state.
	*@param tag Tag string
	*/
    onDocumentChange = (link: string) => {
        this.setState({ documentLink: link })
    }

    /**
       *Sets state for showing alert notification.
       *@param content Notification message
       *@param type Boolean value indicating 1- Success
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
	*Sets state of documentsList by adding new document.
	*/
    onDocumentAddClick = () => {
        if (this.validateLink()) {
            let documentsList = this.state.documentsList;
            documentsList.push(this.state.documentLink);
            this.setState({ documentsList: documentsList, documentLink: "" });
        }
    }

    handleSubmit = () => {
        if (this.checkIfSubmitAllowed()) {
            this.setState({ submitLoading: true });
            let category = this.state.categories.filter(row => row.categoryName === this.state.selectedCategory).shift();
            let idea: IdeaEntity = {
                category: this.state.selectedCategory,
                categoryId: category?.categoryId,
                createdByObjectId: this.userObjectId,
                description: this.state.ideaDescription,
                title: this.state.ideaTitle,
                documentLinks: JSON.stringify(this.state.documentsList),
                tags: this.state.tagsList.join(";"),
                status: ApprovalStatus.Pending,
                totalVotes: undefined,
                approvedOrRejectedByName: undefined,
                approverOrRejecterUserId: undefined,
                createdByName: undefined,
                createdByUserPrincipalName: this.upn,
                createdDate: undefined,
                feedback: undefined,
                ideaId: undefined,
                updatedDate: undefined
            }

            // Post idea
            this.createNewIdea(idea);
        }
    }

    /**
    *Get ideas from API
    */
    async createNewIdea(idea: any) {
        let response = await addNewPostContent(idea);

        if (response.status === 200 && response.data) {
            this.showAlert(this.localize("ideaSubmittedSuccessMessage"), 1);
            this.setState({ submitLoading: false, isIdeaSubmitedsuccessfully: true });
        }
    }

    validateLink = () => {
        let expression = Constants.urlValidationRegEx;
        let regex = new RegExp(expression);
        if (this.state.documentLink.match(regex)) {
            this.setState({ isLinkValid: true })
            return true;
        }
        else {
            this.setState({ isLinkValid: false })
            return false;
        }
    }

    /**
   * Adds document link when enter key is pressed
   * @param event Object containing event details
   */
    onDocumentKeyDown = (event: any) => {
        if (event.key === 'Enter') {
            this.onDocumentAddClick();
        }
    }

    onDocumentRemoveClick = (index: number) => {
        let documents = this.state.documentsList;
        documents.splice(index, 1);
        this.setState({ documentsList: documents });
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

   /**
  *Returns text component containing error message for failed name field validation
  *@param {boolean} isValidLink Indicates whether value is present
  */
    private getInValidLinkError = (isValidLink: boolean) => {
        if (!isValidLink) {
            return (<Text content={this.localize('inValidLinkError')} className="field-error-message" error size="medium" />);
        }

        return (<></>);
    }

    /**
     * Renders the component.
    */
    public render(): JSX.Element {
        if (!this.state.loading && !this.state.isIdeaSubmitedsuccessfully) {
            return (
                <Provider>
                    <div className="module-container">
                        <Flex className="tab-container" column gap="gap.smaller">
                            <Flex className="top-spacing">
                                <Text size="small" content={"*" + this.localize("titleFormLabel")} />
                                <Flex.Item push>
                                    {this.getRequiredFieldError(this.state.isTitlePresent)}
                                </Flex.Item>
                            </Flex>
                            <Flex gap="gap.smaller">
                                <Flex.Item>
                                    <Input
                                        fluid maxLength={200}
                                        placeholder={this.localize("nameIdeaPlaceholder")}
                                        value={this.state.ideaTitle}
                                        onChange={(event: any) => this.onTitleChange(event.target.value)}
                                    />
                                </Flex.Item>
                            </Flex>
                            <Flex className="add-toppadding">
                                <Text size="small" content={"*" + this.localize("synopsisTitle")} />
                                <Flex.Item push>
                                    {this.getRequiredFieldError(this.state.isDescriptionPresent)}
                                </Flex.Item>
                            </Flex>
                            <Flex>
                                <Flex.Item>
                                    <TextArea
                                        maxLength={500} className="response-text-area" fluid
                                        value={this.state.ideaDescription}
                                        placeholder={this.localize("synopsisPlaceholder")}
                                        onChange={(event: any) => this.onDescriptionChange(event.target.value)}
                                    />
                                </Flex.Item>
                            </Flex>
                            <Flex className="add-toppadding">
                                <Text size="small" content={"*" + this.localize("category")} />
                                <Flex.Item push>
                                    {this.getRequiredFieldError(this.state.isCategorySelected)}
                                </Flex.Item>
                            </Flex>
                            <Dropdown fluid
                                items={this.state.categories.map((category) => category.categoryName)}
                                value={this.state.selectedCategory}
                                placeholder={this.localize("categoryPlaceholder")}
                                getA11ySelectionMessage={this.getA11SelectionMessage}
                            />
                            <div>
                                <Flex gap="gap.smaller" className="add-toppadding">
                                    <Text size="small" content={this.localize("tagsFormLabel")} />
                                    <InfoIcon outline className="info-icon" title={this.localize("tagInfo")} size="small" />
                                    <Flex.Item push>
                                        <div>
                                            {this.getTagError()}
                                        </div>
                                    </Flex.Item>
                                </Flex>
                                <Flex gap="gap.smaller" vAlign="center" className="margin-top-small" >
                                    <Input maxLength={Constants.tagMaxLength} placeholder={this.localize("tagPlaceholder")} fluid value={this.state.tag} onKeyDown={this.onTagKeyDown} onChange={(event: any) => this.onTagChange(event.target.value)} />
                                    <Flex.Item push>
                                        <div></div>
                                    </Flex.Item>
                                    <AddIcon key="search" onClick={this.onTagAddClick} className="add-icon hover-effect" />
                                </Flex>
                                <Flex gap="gap.smaller" vAlign="center">
                                    <div>
                                        {
                                            this.state.tagsList.map((value: string, index) => {
                                                if (value.trim().length > 0) {
                                                    return (
                                                        <Label
                                                            circular
                                                            content={<Text className="tag-text-form" content={value.trim()} title={value.trim()} size="small" />}
                                                            className={this.state.theme === Constants.dark ? "tags-label-wrapper-dark" : "tags-label-wrapper"}
                                                            icon={<CloseIcon key={index} className="hover-effect" onClick={() => this.onTagRemoveClick(index)} />}
                                                        />
                                                    )
                                                }
                                            })
                                        }
                                    </div>
                                </Flex>
                            </div>
                            <div>
                                <Flex gap="gap.smaller" className="add-toppadding">
                                    <Text size="small" content={this.localize("documentsFormLabel")} />
                                    <Flex.Item push>
                                        <div>
                                            {this.getInValidLinkError(this.state.isLinkValid)}
                                        </div>
                                    </Flex.Item>
                                </Flex>
                                <Flex gap="gap.smaller" className="margin-top-small" vAlign="center">
                                    <Input placeholder={this.localize("documentPlaceholder")} fluid value={this.state.documentLink} onKeyDown={this.onDocumentKeyDown}
                                        onChange={(event: any) => this.onDocumentChange(event.target.value)} />
                                    <Flex.Item push>
                                        <div></div>
                                    </Flex.Item>
                                    <AddIcon key="search" onClick={this.onDocumentAddClick} className="add-icon hover-effect" />
                                </Flex>
                                <div className="document-text">
                                    {
                                        this.state.documentsList.map((value: string, index) => {
                                            if (value.trim().length > 0) {
                                                return (
                                                    <Flex vAlign="center" key={index} className="margin-top-medium">
                                                        <Text color="blue" className="document-hover" styles={{ paddingRight: "0.3rem" }} truncated content={value.trim()} />
                                                        <Flex.Item align="center">
                                                            <TrashCanIcon outline styles={{ paddingRight: "0.5rem" }} className="hover-effect" onClick={() => this.onDocumentRemoveClick(index)} />
                                                        </Flex.Item>
                                                    </Flex>
                                                )
                                            }
                                        })
                                    }
                                </div>
                            </div>
                        </Flex>
                        <Flex className="tab-footer" hAlign="end" ><Button primary content={this.localize("submitIdeaButtonText")}
                            onClick={this.handleSubmit}
                            disabled={this.state.submitLoading}
                            loading={this.state.submitLoading} />
                        </Flex>
                    </div>

                </Provider>)
        }
        else if (this.state.isIdeaSubmitedsuccessfully) {
            return (
                <div className="submit-idea-success-message-container">
                    <Flex column gap="gap.small">
                        <Flex hAlign="center" className="margin-space"><Image className="preview-image-icon" fluid src={this.appUrl + "/Artifacts/successIcon.png"} /></Flex>
                        <Flex hAlign="center" className="space" column>
                            <Text weight="bold"
                                content={this.localize("ideaPostedSuccessHeading")}
                                size="medium"
                            />
                            <Text
                                content={this.localize("ideaSubmittedSuccessMessage")}
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

export default withTranslation()(SubmitIdea)