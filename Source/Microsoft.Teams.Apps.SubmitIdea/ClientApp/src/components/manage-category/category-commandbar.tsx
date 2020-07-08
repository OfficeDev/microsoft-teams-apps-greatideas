// <copyright file="manage-awards-command-bar.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Button, Input, Flex, AddIcon, EditIcon, TrashCanIcon, SearchIcon } from "@fluentui/react-northstar";
import { withTranslation, WithTranslation } from "react-i18next";


interface ICommandBarProps extends WithTranslation {
    isEditEnable: boolean;
    isDeleteEnable: boolean;
    onAddButtonClick: () => void;
    onEditButtonClick: () => void;
    onDeleteButtonClick: () => void;
    handleTableFilter: (searchText: string) => void;
    isAddEnabled: boolean;
}

interface ICommandbarState {
    searchValue: string
}

class CommandBar extends React.Component<ICommandBarProps, ICommandbarState> {

    constructor(props: ICommandBarProps) {
        super(props);
        this.state = { searchValue: "" };
        this.handleChange = this.handleChange.bind(this);
        this.handleKeyPress = this.handleKeyPress.bind(this);
    }

	/**
	* Set State value of text box input control
	* @param  {Any} event Event object
	*/
    handleChange(event: any) {
        this.setState({ searchValue: event.target.value });
        if (event.target.value.length > 2 || event.target.value === "") {
            this.props.handleTableFilter(event.target.value);
        }
    }

	/**
	* Used to call parent search method on enter key press in text box
	* @param  {Any} event Event object
	*/
    handleKeyPress(event: any) {
        let keyCode = event.which || event.keyCode;
        if (keyCode === 13) {
            if (event.target.value.length > 2 || event.target.value === "") {
                this.props.handleTableFilter(event.target.value);
            }
        }
    }

	/**
	* Renders the component
	*/
    public render(): JSX.Element {
        const { t } = this.props;
        return (
            <Flex gap="gap.small" className="commandbar-wrapper">
                <Button icon={ <AddIcon />} content={t('addButtonText')} text  disabled={!this.props.isAddEnabled} className="add-new-button" onClick={() => this.props.onAddButtonClick()} />
                <Button icon={ <EditIcon />} content={t('editButtonText')} text disabled={!this.props.isEditEnable} className="edit-button" onClick={() => this.props.onEditButtonClick()} />
                <Button icon={ <TrashCanIcon/>} content={t('deleteButtonText')} text disabled={!this.props.isDeleteEnable} className="delete-button" onClick={() => this.props.onDeleteButtonClick()} />
                <Flex.Item push>
                    <div className="search-bar-margin">
                        <Input
                            icon={ <SearchIcon /> }
                            fluid placeholder={t('searchPlaceholder')}
                            value={this.state.searchValue}
                            onChange={this.handleChange}
                            onKeyUp={this.handleKeyPress}
                        />
                    </div>
                </Flex.Item>
            </Flex>
        );
    }
}

export default withTranslation()(CommandBar)