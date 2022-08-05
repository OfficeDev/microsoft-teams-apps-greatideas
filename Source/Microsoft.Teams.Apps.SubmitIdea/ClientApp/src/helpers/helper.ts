﻿// <copyright file="helper.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import { IPostType } from "../constants/resources";
import Resources from "../constants/resources";
import { TFunction } from "i18next";
import { getPreferencecategories } from "../api/preferences-api";

/**
 * Get localized post types.
 * @param localize i18n TFunction received from props.
 */
export const getLocalizedPostTypes = (
  localize: TFunction
): Array<IPostType> => {
  return Resources.postTypes.map((value: IPostType) => {
    switch (value.id) {
      case "1":
        value.name = localize("blogPostType");
        return value;
      case "2":
        value.name = localize("otherPostType");
        return value;
      case "3":
        value.name = localize("podCasePostType");
        return value;
      case "4":
        value.name = localize("videoPostType");
        return value;
      case "5":
        value.name = localize("bookPostType");
        return value;
      default:
        return value;
    }
  });
};

/**
 * Get localized sort by filters.
 * @param localize i18n TFunction received from props.
 */
export const getLocalizedSortBy = (localize: TFunction): Array<IPostType> => {
  return Resources.sortBy.map((value: IPostType) => {
    switch (value.id) {
      case "0":
        value.name = localize("sortByNewest");
        return value;
      case "1":
        value.name = localize("sortByPopularity");
        return value;
      default:
        return value;
    }
  });
};

export const isNullorWhiteSpace = (input: string): boolean => {
  return !input || !input.trim();
};

/**
 * Get random colors for avatar.
 */
export const generateColor = () => {
  return Resources.avatarColors[
    Math.floor(Math.random() * Resources.avatarColors.length)
  ];
};

/**
 * get initial of user names to show in avatar.
 */
export const getInitials = (userPostName: string) => {
  let fullName = userPostName;
  let names = fullName?.split(" "),
    initials = names &&names.length > 1 ?names[0]?.substring(0, 1).toUpperCase():"";

  if (names && names.length > 1) {
    initials += names[names.length - 1].substring(0, 1).toUpperCase();
  }
  return initials;
};

/**
 * Get all the saved categories which user selected previously
 */
export const getPreferenceDetails = async (teamId: string) => {
  let response = await getPreferencecategories(teamId);
  let result: any;
  if (response.status === 200 && response.data) {
    result = {
      categories: response.data.categories.split(";"),
      frequency: response.data.digestFrequency,
    };
    return result;
  }
};
