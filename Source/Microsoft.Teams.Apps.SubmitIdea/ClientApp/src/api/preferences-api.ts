// <copyright file="preferences-api.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import axios from "./axios-decorator";
import { getBaseUrl } from '../configVariables';

let baseAxiosUrl = getBaseUrl() + '/api';

/**
* Get preferences categories for configure preferences
* @param teamId Team Id for which user configured tags needs to be fetched
*/
export const getPreferencecategories = async (teamId: string): Promise<any> => {
    let url = `${baseAxiosUrl}/teampreference?teamId=${teamId}`;
    return await axios.get(url);
}