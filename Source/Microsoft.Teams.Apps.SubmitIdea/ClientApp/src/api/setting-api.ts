// <copyright file="setting-api.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import axios from "./axios-decorator";
import { getBaseUrl } from '../configVariables';

const baseAxiosUrl = getBaseUrl() + '/api';

/**
* Get bot application settings from API.
*/
export const getBotSetting = async (teamId: string | undefined): Promise<any> => {

    let url = baseAxiosUrl + `/Settings/botsettings?teamId=${teamId}`;
    return await axios.get(url, undefined);
}