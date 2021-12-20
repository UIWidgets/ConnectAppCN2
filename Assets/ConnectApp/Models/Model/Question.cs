using System;
using System.Collections.Generic;
using ConnectApp.Components.Markdown.basic;

namespace ConnectApp.Models.Model {
    public enum QuestionStatus {
        writing,
        verifying,
        rejected,
        published,
        resolved,
        deleted,
        banned
    }

    public enum QATab {
        latest,
        hot,
        pending
    }

    [Serializable]
    public class Question {
        public string id;
        public string title;
        public string authorId;
        public string plateId;
        public List<string> tagIds;
        public string status;
        public DateTime? publishedTime;
        public DateTime? createdTime;
        public DateTime? updatedTime;
        public int followCount;
        public int viewCount;
        public int likeCount; // vote
        public int answerCount;
        public int commentCount;
        public int sharedCount;
        public string description;
        public List<Node> markdownBodyNodes;
        public string descriptionPlain;
        public List<string> contentIds;
        public string channelId;
        public DateTime? deletedTime;
        public string verifiedReason;

        Question copyWith(
            string id = null,
            string title = null,
            string authorId = null,
            string plateId = null,
            List<string> tagIds = null,
            string status = null,
            DateTime? publishedTime = null,
            DateTime? createdTime = null,
            DateTime? updatedTime = null,
            DateTime? deletedTime = null,
            int? followCount = null,
            int? viewCount = null,
            int? likeCount = null, // vote
            int? answerCount = null,
            int? commentCount = null,
            int? sharedCount = null,
            string description = null,
            List<Node> markdownBodyNodes = null,
            string descriptionPlain = null,
            List<string> contentIds = null,
            string channelId = null,
            string verifiedReason = null
        ) {
            return new Question {
                id = id ?? this.id,
                title = title ?? this.title,
                authorId = authorId ?? this.authorId,
                plateId = plateId ?? this.plateId,
                tagIds = tagIds ?? this.tagIds,
                status = status ?? this.status,
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
                description = description ?? this.description,
                markdownBodyNodes = markdownBodyNodes ?? this.markdownBodyNodes,
                descriptionPlain = descriptionPlain ?? this.descriptionPlain,
                contentIds = contentIds ?? this.contentIds,
                channelId = channelId ?? this.channelId,
                verifiedReason = verifiedReason ?? this.verifiedReason
            };
        }

        public Question Merge(Question other) {
            if (null == other) {
                return this;
            }

            return this.copyWith(
                id: other.id,
                title: other.title,
                authorId: other.authorId,
                plateId: other.plateId,
                tagIds: other.tagIds,
                status: other.status,
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
                description: other.description,
                markdownBodyNodes: other.markdownBodyNodes,
                descriptionPlain: other.descriptionPlain,
                contentIds: other.contentIds,
                channelId: other.channelId,
                verifiedReason: other.verifiedReason
            );
        }
    }

    [Serializable]
    public class QAAttachment {
        public string id;
        public string questionsId;
        public string answerId;
        public string userId;
        public string type;
        public OriginalImage originalImage;
        public ThumbnailImage thumbnail;
    }

    [Serializable]
    public class QuestionDraft {
        public string questionId;
        public string title;
        public string description;
        public Plate plate;
        public List<string> tagIds;
        public List<string> contentIds;
    }
}