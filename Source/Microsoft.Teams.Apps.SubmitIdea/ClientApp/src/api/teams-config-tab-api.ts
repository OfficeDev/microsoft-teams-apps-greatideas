// <copyright file="teams-config-tab-api.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import axios from "./axios-decorator";
import { getBaseUrl } from '../configVariables';

let baseAxiosUrl = getBaseUrl() + '/api';

/**
* Post config category for discover tab
* @param postContent Categories to be saved
*/
export const submitConfigCategories = async (postContent: any): Promise<any> => {
    let url = `${baseAxiosUrl}/teamcategory`;
    return await axios.post(url, postContent);
}

/**
* Get preferences category for configure preferences
* @param teamId Team Id for which configured tags needs to be fetched
*/
export const getConfigCategories = async (teamId: string): Promise<any> => {
    let url = `${baseAxiosUrl}/teamcategory?teamId=${teamId}`;
    return await axios.get(url);
}

/** * Update preferences category for configure preferences
 * @param postContent Categories to be saved
*/export const updateConfigCategories = async (postContent: any): Promise<any> => {
    let url = `${baseAxiosUrl}/teamcategory`;
    return await axios.patch(url, postContent);
}