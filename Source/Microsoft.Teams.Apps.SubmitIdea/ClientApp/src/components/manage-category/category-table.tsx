// <copyright file="categorys-table.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Table, Text } from "@fluentui/react-northstar";
import CheckboxBase from "./checkbox-base";
import { useTranslation } from 'react-i18next';
import { ICategoryDetails } from "../models/category";

import "../../styles/site.css";

interface ICategoryTableProps {
    showCheckbox: boolean,
    categories: ICategoryDetails[],
    onCheckBoxChecked: (categoryId: string, isChecked: boolean) => void,
}

const CategoryTable: React.FunctionComponent<ICategoryTableProps> = props => {
    const { t } = useTranslation();
    const categoryTableHeader = {
        key: "header",
        items: props.showCheckbox === true ?
            [
                { content: <div />, key: "check-box", className: "table-checkbox-cell" },
                {
                    content: <Text weight="semibold" content={t('categoryName')} />, key: "name", className: "category-table-name"
                },
                { content: <Text weight="semibold" content={t('categoryDescription')} />, key: "description", className: "category-table-description" }
            ]
            :
            [
                { content: <Text weight="semibold" content={t('categoryName')} />, key: "name", className: "category-table-name" },
                { content: <Text weight="semibold" content={t('categoryDescription')} />, key: "description", className: "category-table-description" }
            ],
    };

    let categoryTableRows = props.categories.map((category, index) => (
        {
            key: index,
            style: {},
            items: props.showCheckbox === true ?
                [
                    { content: <CheckboxBase onCheckboxChecked={props.onCheckBoxChecked} value={category.categoryId!} />, key: index + "1", className: "table-checkbox-cell" },
                    { content: <Text content={category.categoryName} title={category.categoryName} />, key: index + "3", truncateContent: true, className: "category-table-name" },
                    { content: <Text content={category.categoryDescription} title={category.categoryDescription} />, key: index + "4", truncateContent: true, className: "category-table-description" }
                ]
                :
                [
                    { content: <Text content={category.categoryName} title={category.categoryName} />, key: index + "2", truncateContent: true, className: "category-table-name" },
                    { content: <Text content={category.categoryDescription} title={category.categoryDescription} />, key: index + "3", truncateContent: true, className: "category-table-description" }
                ],
        }
    ));

    return (
        <div>
            <Table rows={categoryTableRows}
                header={categoryTableHeader} className="table-cell-content" />
        </div>
    );
}

export default CategoryTable;