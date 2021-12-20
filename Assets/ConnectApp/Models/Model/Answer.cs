using System;
using System.Collections.Generic;
using ConnectApp.Components.Markdown.basic;

namespace ConnectApp.Models.Model {
    public enum AnswerStatus {
        writing,
        verifying,
        rejected,
        published,
        deleted,
        banned
    }

    [Serializable]
    public class Answer {
        public string id;
        public string questionId;
        public string descriptionPlain;
        public string description;
        public List<Node> markdownBodyNodes;
        public List<string> contentIds;
        public string authorId;
        public string status;
        public bool isAccepted;
        public DateTime? publishedTime;
        public DateTime? createdTime;
        public DateTime? updatedTime;
        public DateTime? deletedTime;
        public int followCount;
        public int viewCount;
        public int likeCount;
        public int answerCount;
        public int commentCount;
        public int sharedCount;
        public string channelId;
        public string verifiedReason;

        Answer copyWith(
            string id = null,
            string questionId = null,
            string descriptionPlain = null,
            string description = null,
            List<Node> markdownBodyNodes = null,
            List<string> contentIds = null,
            string authorId = null,
            string status = null,
            bool? isAccepted = null,
            DateTime? publishedTime = null,
            DateTime? createdTime = null,
            DateTime? updatedTime = null,
            DateTime? deletedTime = null,
            int? followCount = null,
            int? viewCount = null,
            int? likeCount = null,
            int? answerCount = null,
            int? commentCount = null,
            int? sharedCount = null,
            string channelId = null,
            string verifiedReason = null
        ) {
            return new Answer {
                id = id ?? this.id,
                questionId = questionId ?? this.questionId,
                descriptionPlain = descriptionPlain ?? this.descriptionPlain,
                description = description ?? this.description,
                markdownBodyNodes = markdownBodyNodes ?? this.markdownBodyNodes,
                contentIds = contentIds ?? this.contentIds,
                authorId = authorId ?? this.authorId,
                status = status ?? this.status,
                isAccepted = isAccepted ?? this.isAccepted,
                publishedTime = publishedTime ?? this.publishedTime,
                createdTime = createdTime ?? this.createdTime,
                updatedTime = updatedTime ?? this.updatedTime,
                deletedTime = deletedTime ?? this.deletedTime,
                followCount = followCount ?? this.followCount,
                viewCount = viewCount ?? this.viewCount,
                likeCount = likeCount ?? this.likeCount,
                answerCount = answerCount ?? this.answerCount,
                commentCount = commentCount ?? this.commentCount,
                sharedCount = sharedCount ?? this.sharedCount,
                channelId = channelId ?? this.channelId,
                verifiedReason = verifiedReason ?? this.verifiedReason
            };
        }

        public Answer Merge(Answer other) {
            if (null == other) {
                return this;
            }

            return this.copyWith(
                id: other.id,
                questionId: other.questionId,
                descriptionPlain: other.descriptionPlain,
                description: other.description,
                markdownBodyNodes: other.markdownBodyNodes,
                contentIds: other.contentIds,
                authorId: other.authorId,
                status: other.status,
                isAccepted: other.isAccepted,
                publishedTime: other.publishedTime,
                createdTime: other.createdTime,
                updatedTime: other.updatedTime,
                deletedTime: other.deletedTime,
                followCount: other.followCount,
                viewCount: other.viewCount,
                likeCount: other.likeCount,
                answerCount: other.answerCount,
                commentCount: other.commentCount,
                sharedCount: other.sharedCount,
                channelId: other.channelId,
                verifiedReason: other.verifiedReason
            );
        }
    }

    [Serializable]
    public class AnswerDraft {
        public string answerId;
        public string questionId;
        public string questionTitle;
        public string description;
        public string descriptionPlain;
        public DateTime? createdTime;
        public List<string> contentIds;
    }
}