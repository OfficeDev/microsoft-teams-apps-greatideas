// <copyright file="categoy-api.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import axios from "./axios-decorator";

import { getBaseUrl } from '../configVariables';

const baseAxiosUrl = getBaseUrl() + '/api';

/**
* Get all categories data from API
*/
export const getAllCategories = async (): Promise<any> => {

    let url = baseAxiosUrl + `/Category/allcategories`;
    return await axios.get(url, undefined);
}

/**
* Post category data to API
* @param data Post details object to be updated
*/
export const postCategory = async (data: any): Promise<any> => {

    let url = baseAxiosUrl + '/Category/';
    return await axios.post(url, data, undefined);
}

/**
* Update category details.
* @param data Patch details object to be updated
*/
export const updateCategory = async (data: any): Promise<any> => {

    let url = `${baseAxiosUrl}/Category/`;
    return await axios.patch(url, data);
}

/**
* Delete user selected category
* @param {string} categoryIds selected category ids which needs to be deleted
*/
export const deleteSelectedCategories = async (categoryIds: string): Promise<any> => {

    let url = baseAxiosUrl + `/Category/categories?categoryIds=${categoryIds}`;
    return await axios.delete(url);
}