// <copyright file="command-bar.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Flex, Input, Button, Text } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";
import { SearchIcon, CanvasAddPageIcon } from "@fluentui/react-icons-northstar";
import { Icon } from "@fluentui/react/lib/Icon";
import { initializeIcons } from "@uifabric/icons";
import { useTranslation } from 'react-i18next';

import "../../styles/command-bar.css";

interface ICommandBarProps {
    onFilterButtonClick: () => void;
    onSearchInputChange: (searchString: string) => void;
    searchFilterPostsUsingAPI: () => void;
    commandBarSearchText: string;
    showSolidFilterIcon: boolean;
    showAddNewButton: boolean;
    botId?: string;
}

const CommandBar: React.FunctionComponent<ICommandBarProps> = props => {
    const localize = useTranslation().t;
    initializeIcons();

    /**
	* Invokes for key press
	* @param event Object containing event details
	*/
    const onTagKeyDown = (event: any) => {
        if (event.key === 'Enter') {
            props.searchFilterPostsUsingAPI();
        }
    }

    /**
    *Navigate to submit idea task module.
    */
    const handleAddClick = () => {
        let appBaseUrl = window.location.origin;
        microsoftTeams.tasks.startTask({
            completionBotId: props.botId,
            title: localize('submitIdeaTaskModuleHeaderText'),
            height: 720,
            width: 700,
            url: `${appBaseUrl}/submit-idea`,
            fallbackUrl: `${appBaseUrl}/submit-idea`,
        }, submitHandler);
    }

    /**
    * Submit idea task module handler.
    */
    const submitHandler = async (error, result) => {
        if (error) {
            console.log(error);
        }
    };

    return (
        <Flex gap="gap.small" vAlign="center" hAlign="end" className="command-bar-wrapper">
            <Flex.Item push>
                <Button className="filter-button" icon={props.showSolidFilterIcon ? <Icon iconName="FilterSolid" className="filter-icon-filled" /> : <Icon iconName="Filter" className="filter-icon" />} content={<Text content={localize("filter")} className={props.showSolidFilterIcon ? "filter-icon-filled": ""} />} text onClick={props.onFilterButtonClick} />
            </Flex.Item>
            <div className="search-bar-wrapper">
                <Input inverted fluid onKeyDown={onTagKeyDown} onChange={(event: any) => props.onSearchInputChange(event.target.value)} value={props.commandBarSearchText} placeholder={localize("searchPlaceholder")} />
                <SearchIcon key="search" onClick={(event: any) => props.searchFilterPostsUsingAPI()} className="discover-search-icon" />
            </div>
            {props.showAddNewButton && <Button icon={<CanvasAddPageIcon />} primary content={localize("addButtonText")} onClick={handleAddClick} />}
        </Flex>
    );
}

export default CommandBar;