// <copyright file="router.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Suspense } from "react";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import IdeaWrapperPage from "../components/card-view/idea-wrapper-page";
import TeamsIdeaWrapperPage from "../components/card-view/teams-idea-wrapper-page";
import SignInPage from "../components/signin/signin";
import SignInSimpleStart from "../components/signin/signin-start";
import SignInSimpleEnd from "../components/signin/signin-end";
import ConfigurePreferences from "../components/configure-preference-dialog/configure-preference";
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
    <Routes>
      <Route path="/ideas" element={<IdeaWrapperPage />} />
      <Route path="/team-ideas" element={<TeamsIdeaWrapperPage />} />
      <Route path="/signin" element={<SignInPage />} />
      <Route path="/signin-simple-start" element={<SignInSimpleStart />} />[]
      <Route path="/signin-simple-end" element={<SignInSimpleEnd />} />
      <Route path="/configure-preferences" element={<ConfigurePreferences />} />
      <Route path="/error" element={<ErrorPage />} />
      <Route path="/curator-config-tab" element={<CuratorTeamConfig />} />
      <Route path="/curator-dashboard" element={<CuratorTeamDashBoard />} />
      <Route path="/manage-category" element={<ManageCategory />} />
      <Route path="/submit-idea" element={<SubmitIdea />} />
      <Route path="/view-idea" element={<ViewIdea />} />
      <Route path="/upvote-idea" element={<UpvoteIdea />} />
      <Route path="/user-config-tab" element={<UserTeamConfigTab />} />
      <Route element={<Redirect />} />
    </Routes>
  );
};
