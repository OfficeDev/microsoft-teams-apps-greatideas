// <copyright file="discover-api.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import axios from "./axios-decorator";
import { getBaseUrl } from '../configVariables';

let baseAxiosUrl = getBaseUrl() + '/api';

/**
* Get idea details from API
* @param ideaId Unique id of idea to get fetch data.
*/
export const getIdea = async (userId: string, ideaId: string): Promise<any> => {
    let url = `${baseAxiosUrl}/idea/idea?userId=${userId}&ideaId=${ideaId}`;
    return await axios.get(url);
}

/**
* Get discover posts for tab
* @param pageCount Current page count for which posts needs to be fetched
*/
export const getAllIdeas = async (pageCount: number): Promise<any> => {

    let url = `${baseAxiosUrl}/idea?pageCount=${pageCount}`;
    return await axios.get(url);
}

/**
* Get discover posts for tab in a team
* @param teamId Team Id for which discover posts needs to be fetched
* @param pageCount Current page count for which posts needs to be fetched
*/
export const getTeamDiscoverPosts = async (teamId: string, pageCount: number): Promise<any> => {

    let url = `${baseAxiosUrl}/teamidea/team-ideas?teamId=${teamId}&pageCount=${pageCount}`;
    return await axios.get(url);
}

/**
* Get filtered discover posts for tab
* @param postTypes Selected post types separated by semicolon
* @param sharedByNames Selected author names separated by semicolon
* @param tags Selected tags separated by semicolon
* @param sortBy Sort post by
* @param teamId Team Id for which posts needs to be fetched
* @param pageCount Current page count for which posts needs to be fetched
*/
export const getFilteredPosts = async (categories: string, sharedByNames: string, tags: string, sortBy: string, teamId: string, pageCount: number): Promise<any> => {
    let url = `${baseAxiosUrl}/teamidea/applied-filtered-team-ideas?categories=${encodeURIComponent(categories)}&sharedByNames=${sharedByNames}
                &tags=${encodeURIComponent(tags)}&sortBy=${sortBy}&teamId=${teamId}&pageCount=${pageCount}`;
    return await axios.get(url);
}

/**
* Get unique tags
*/
export const getTags = async (): Promise<any> => {
    let url = `${baseAxiosUrl}/teampreference/unique-tags?searchText=*`;
    return await axios.get(url);
}

/**
* Get unique categories
*/
export const getCategories = async (): Promise<any> => {
    let url = `${baseAxiosUrl}/idea/unique-categories?searchText=*`;
    return await axios.get(url);
}

/**
* Update post content details
* @param postContent Post details object to be updated
*/
export const updatePostContent = async (postContent: any): Promise<any> => {

    let url = `${baseAxiosUrl}/idea`;
    return await axios.patch(url, postContent);
}

/**
* Add new post
* @param postContent Post details object to be added
*/
export const addNewPostContent = async (postContent: any): Promise<any> => {

    let url = `${baseAxiosUrl}/idea`;
    return await axios.post(url, postContent);
}

/**
* Get user votes from storage
* @param post Id of post to be deleted
*/
export const getUserVotes = async (): Promise<any> => {

    let url = `${baseAxiosUrl}/uservote/votes`;
    return await axios.get(url);
}

/**
* Add user vote
* @param userVote Vote object to be added in storage
*/
export const addUserVote = async (userVote: any): Promise<any> => {

    let url = `${baseAxiosUrl}/uservote/vote?postCreatedByuserId=${userVote.userId}&postId=${userVote.postId}`;
    return await axios.post(url);
}

/**
* delete user vote
* @param userVote Vote object to be deleted from storage
*/
export const deleteUserVote = async (userVote: any): Promise<any> => {

    let url = `${baseAxiosUrl}/uservote?postCreatedByuserId=${userVote.userId}&postId=${userVote.postId}`;
    return await axios.delete(url);
}

/**
* Get list of authors
*/
export const getAuthors = async (): Promise<any> => {

    let url = `${baseAxiosUrl}/idea/unique-user-names`;
    return await axios.get(url);
}

/**
* Add new post
* @param searchText Search text typed by user
* @param pageCount Current page count for which posts needs to be fetched
*/
export const filterTitleAndTags = async (searchText: string, pageCount: number): Promise<any> => {
    let url = baseAxiosUrl + `/idea/search-ideas?searchText=${encodeURIComponent(searchText)}&pageCount=${pageCount}`;
    return await axios.get(url);
}

/**
* Add new post
* @param searchText Search text typed by user
* @param teamId Team Id for which post needs to be filtered
* @param pageCount Current page count for which posts needs to be fetched
*/
export const filterTitleAndTagsTeam = async (searchText: string, teamId: string, pageCount: number): Promise<any> => {
    let url = baseAxiosUrl + `/teamidea/team-search-ideas?searchText=${encodeURIComponent(searchText)}&teamId=${teamId}&pageCount=${pageCount}`;
    return await axios.get(url);
}

/**
* Get configured tags for a team.
* @param teamId Team Id for which configuration needs to be fetched
*/
export const getTeamConfiguredTags = async (teamId: string): Promise<any> => {
    let url = `${baseAxiosUrl}/teamidea/tags-for-categories?teamId=${teamId}`;
    return await axios.get(url);
}

/**
* Get list of authors based on the configured tags in a team.
* @param teamId Team Id for which authors needs to be fetched
*/
export const getTeamAuthorsData = async (teamId: string): Promise<any> => {

    let url = `${baseAxiosUrl}/teamidea/authors-for-categories?teamId=${teamId}`;
    return await axios.get(url);
}

/**
* Get configured categories for a team.
* @param teamId Team Id for which configuration needs to be fetched
*/
export const getTeamConfiguredCategoriess = async (teamId: string): Promise<any> => {
    let url = `${baseAxiosUrl}/teamidea/team-unique-categories?teamId=${teamId}`;
    return await axios.get(url);
}