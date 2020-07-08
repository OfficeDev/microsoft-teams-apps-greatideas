// <copyright file="app-insights.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import { ApplicationInsights } from "@microsoft/applicationinsights-web";
import { ReactPlugin } from "@microsoft/applicationinsights-react-js";

export const getApplicationInsightsInstance = (telemetry: any, browserHistory: any): any => {
    // Initialize application insights for logging events and errors.

    let reactPlugin = new ReactPlugin();
    let appInsights: any = null;
    try {
        appInsights = new ApplicationInsights({
            config: {
                instrumentationKey: telemetry,
                extensions: [reactPlugin],
                extensionConfig: {
                    [reactPlugin.identifier]: { history: browserHistory }
                }
            }
        });
        appInsights.loadAppInsights();
        return appInsights;
    }
    catch (e) {
        appInsights = new ApplicationInsights({
            config: {
                instrumentationKey: undefined,
                extensions: [reactPlugin],
                extensionConfig: {
                    [reactPlugin.identifier]: { history: browserHistory }
                }
            }
        });
        console.log(e);
        return appInsights;
    }
}