/*
    <copyright file="category.ts" company="Microsoft">
    Copyright (c) Microsoft. All rights reserved.
    </copyright>
*/

export class CategoryDetails {
    categoryId: string | undefined;
    categoryName: string | undefined;
    categoryDescription: string | undefined;
    createdByUserId: string | undefined;
    modifiedByUserId: string | undefined;
    createdOn: Date | undefined;
}

export interface ICategoryDetails {
    categoryId: string | undefined;
    categoryName: string | undefined;
    categoryDescription: string | undefined;
    createdByUserId: string | undefined;
    modifiedByUserId: string | undefined;
    createdOn: Date | undefined;
    timestamp: string;
}