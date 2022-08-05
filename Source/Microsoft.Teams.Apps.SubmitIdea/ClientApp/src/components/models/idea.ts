/*
    <copyright file="idea.ts" company="Microsoft">
    Copyright (c) Microsoft. All rights reserved.
    </copyright>
*/

export class IdeaEntity {
    ideaId: string | undefined;
    title: string | undefined;
    description: string | undefined;
    category: string | undefined;
    categoryId: string | undefined;
    tags: string | undefined;
    createdDate: Date | undefined;
    createdByName: string | undefined;
    createdByUserPrincipalName: string | undefined;
    updatedDate: Date | undefined;
    createdByObjectId: string | undefined;
    totalVotes: number | undefined;
    documentLinks: string | undefined;
    approvedOrRejectedByName: string | undefined;
    approverOrRejecterUserId: string | undefined;
    status: number | undefined;
    feedback: string | undefined;
    backgroundColor?: string | undefined;
}

//Enhancement: Added Accepted Status Enum
export enum ApprovalStatus {
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Accepted = 3
}

export class UpvoteEntity {
    postId: string | undefined;
    userId: string | undefined;
}