// <copyright file="category-label.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Text, Status, Flex } from "@fluentui/react-northstar";
import { generateColor } from "../../helpers/helper";


interface ITypeLabelProps {
    categoryName: string;
}

const CategoryLabel: React.FunctionComponent<ITypeLabelProps> = props => {
    const categoryColor: string = generateColor();

    return (
        <Flex vAlign="center">
            <Status styles={{ backgroundColor: categoryColor }} />&nbsp;<Text content={props.categoryName} title={props.categoryName} size="small" />
        </Flex>
    );
}

export default React.memo(CategoryLabel);