// <copyright file="command-bar.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Flex, Input, Button } from "@fluentui/react-northstar";
import { SearchIcon } from "@fluentui/react-icons-northstar";
import { initializeIcons } from "@uifabric/icons";
import { useTranslation } from 'react-i18next';

import "../../styles/command-bar.css";

interface ICommandBarProps {
    onSearchInputChange: (searchString: string) => void;
    searchFilterPostsUsingAPI: () => void;
    commandBarSearchText: string;
    onManageCategoryButtonClick: () => void;
}

const CuratorCommandBar: React.FunctionComponent<ICommandBarProps> = props => {
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

    return (
        <Flex gap="gap.small" vAlign="center" hAlign="end" className="title-bar-wrapper">
            <Button content={localize("manageButtonText")} primary onClick={props.onManageCategoryButtonClick} />
            <Flex.Item push>
                <div className="search-bar-wrapper">
                    <Input inverted fluid onKeyDown={onTagKeyDown} onChange={(event: any) => props.onSearchInputChange(event.target.value)} value={props.commandBarSearchText} placeholder={localize("searchPlaceholder")} />
                    <SearchIcon key="search" onClick={(event: any) => props.searchFilterPostsUsingAPI()} className="discover-search-icon" />
                </div>
            </Flex.Item>
        </Flex>
    );
}

export default CuratorCommandBar;