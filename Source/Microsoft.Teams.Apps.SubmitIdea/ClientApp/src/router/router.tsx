// <copyright file="router.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Suspense } from "react";
import { BrowserRouter, Route, Switch } from "react-router-dom";
import IdeaWrapperPage from "../components/card-view/idea-wrapper-page";
import TeamsIdeaWrapperPage from "../components/card-view/teams-idea-wrapper-page";
import SignInPage from "../components/signin/signin";
import SignInSimpleStart from "../components/signin/signin-start";
import SignInSimpleEnd from "../components/signin/signin-end";
import configurepreference from "../components/configure-preference-dialog/configure-preference";
import CuratorTeamConfig from "../components/curator-team/curator-config-tab";
import ViewIdea from "../components/curator-team/view-idea";
import UpvoteIdea from "../components/card-view/upvote-idea";
import SubmitIdea from "../components/add-new-dialog/submit-idea";
import CuratorTeamDashBoard from "../components/curator-team/curator-dashboard";
import ManageCategory from "../components/manage-category/manage-category";
import UserTeamConfigTab from "../components/user-team/user-team-config";
import Redirect from "../components/redirect";
import ErrorPage from "../components/error-page";

export const AppRoute: React.FunctionComponent<{}> = () => {

    return (
        <Suspense fallback={<div className="container-div"><div className="container-subdiv"></div></div>}>
            <BrowserRouter>
                <Switch>
                    <Route exact path="/ideas" component={IdeaWrapperPage} />
                    <Route exact path="/team-ideas" component={TeamsIdeaWrapperPage} />
                    <Route exact path="/signin" component={SignInPage} />
                    <Route exact path="/signin-simple-start" component={SignInSimpleStart} />
                    <Route exact path="/signin-simple-end" component={SignInSimpleEnd} />
                    <Route exact path="/configure-preferences" component={configurepreference} />
                    <Route exact path="/error" component={ErrorPage} />
                    <Route exact path="/curator-config-tab" component={CuratorTeamConfig} />
                    <Route exact path="/curator-dashboard" component={CuratorTeamDashBoard} />
                    <Route exact path="/manage-category" component={ManageCategory} />
                    <Route exact path="/submit-idea" component={SubmitIdea} />
                    <Route exact path="/view-idea" component={ViewIdea} />
                    <Route exact path="/upvote-idea" component={UpvoteIdea} />
                    <Route exact path="/user-config-tab" component={UserTeamConfigTab} />
                    <Route component={Redirect} />
                </Switch>
            </BrowserRouter>
        </Suspense>
    );
}
