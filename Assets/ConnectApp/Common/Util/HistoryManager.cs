using System.Collections.Generic;
using ConnectApp.Models.Model;
using Newtonsoft.Json;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace ConnectApp.Common.Util {
    public static class HistoryManager {
        const string _homeAfterTimeKey = "homeAfterTimeKey";
        const string _searchArticleHistoryKey = "searchArticleHistoryKey";
        const string _articleHistoryKey = "articleHistoryKey";
        const string _blockArticleKey = "blockArticleKey";
        const string _blockQuestionKey = "blockQuestionKey";
        const string _blockAnswerKey = "blockAnswerKey";
        const string _blockUserKey = "blockUserKey";
        const string _visitorId = "visitor";
        const int _searchArticleHistoryLimit = 5;
        const int _articleHistoryLimit = 50;

        public static string homeAfterTime(string userId) {
            if (!PlayerPrefs.HasKey(_homeAfterTimeKey + userId)) {
                return "";
            }

            return PlayerPrefs.GetString(_homeAfterTimeKey + userId);
        }

        public static void saveHomeAfterTime(string afterTime, string userId) {
            PlayerPrefs.SetString(_homeAfterTimeKey + userId, value: afterTime);
            PlayerPrefs.Save();
        }

        public static void deleteHomeAfterTime(string userId) {
            if (PlayerPrefs.HasKey(_homeAfterTimeKey + userId)) {
                PlayerPrefs.DeleteKey(_homeAfterTimeKey + userId);
            }
        }

        /// Article History List
        public static List<Article> articleHistoryList(string userId = _visitorId) {
            var articleHistory = PlayerPrefs.GetString(_articleHistoryKey + userId);
            var articleHistoryList = new List<Article>();
            if (articleHistory.isNotEmpty()) {
                articleHistoryList = JsonConvert.DeserializeObject<List<Article>>(value: articleHistory);
            }

            var blockArticleList = new List<string>();
            if (userId != _visitorId) {
                blockArticleList = HistoryManager.blockArticleList(userId: userId);
            }

            blockArticleList.ForEach(articleId => {
                articleHistoryList.RemoveAll(article => article.id == articleId);
            });
            return articleHistoryList;
        }

        public static List<Article> saveArticleHistory(Article article, string userId = _visitorId) {
            var articleHistory = PlayerPrefs.GetString(_articleHistoryKey + userId);
            var articleHistoryList = new List<Article>();
            if (articleHistory.isNotEmpty()) {
                articleHistoryList = JsonConvert.DeserializeObject<List<Article>>(value: articleHistory);
            }
            
            articleHistoryList.RemoveAll(item => item.id == article.id);
            articleHistoryList.Insert(0, item: article);
            if (articleHistoryList.Count > _articleHistoryLimit) {
                articleHistoryList.RemoveRange(index: _articleHistoryLimit,
                    articleHistoryList.Count - _articleHistoryLimit);
            }
            
            var newArticleHistory = JsonConvert.SerializeObject(value: articleHistoryList);
            PlayerPrefs.SetString(_articleHistoryKey + userId, value: newArticleHistory);
            PlayerPrefs.Save();
            return articleHistoryList;
        }

        public static List<Article> deleteArticleHistory(string articleId, string userId = _visitorId) {
            var articleHistory = PlayerPrefs.GetString(_articleHistoryKey + userId);
            var articleHistoryList = new List<Article>();
            if (articleHistory.isNotEmpty()) {
                articleHistoryList = JsonConvert.DeserializeObject<List<Article>>(value: articleHistory);
            }

            articleHistoryList.RemoveAll(item => item.id == articleId);
            var newArticleHistory = JsonConvert.SerializeObject(value: articleHistoryList);
            PlayerPrefs.SetString(_articleHistoryKey + userId, value: newArticleHistory);
            PlayerPrefs.Save();
            return articleHistoryList;
        }

        public static void deleteAllArticleHistory(string userId = _visitorId) {
            if (PlayerPrefs.HasKey(_articleHistoryKey + userId)) {
                PlayerPrefs.DeleteKey(_articleHistoryKey + userId);
            }
        }

        /// Search History List
        public static List<string> searchArticleHistoryList(string userId = _visitorId) {
            var searchArticleHistory = PlayerPrefs.GetString(_searchArticleHistoryKey + userId);
            var searchArticleHistoryList = new List<string>();
            if (searchArticleHistory.isNotEmpty()) {
                searchArticleHistoryList = JsonConvert.DeserializeObject<List<string>>(value: searchArticleHistory);
            }

            return searchArticleHistoryList;
        }

        public static List<string> saveSearchArticleHistoryList(string keyword, string userId = _visitorId) {
            var searchArticleHistory = PlayerPrefs.GetString(_searchArticleHistoryKey + userId);
            var searchArticleHistoryList = new List<string>();
            if (searchArticleHistory.isNotEmpty()) {
                searchArticleHistoryList = JsonConvert.DeserializeObject<List<string>>(value: searchArticleHistory);
            }

            if (searchArticleHistoryList.Contains(item: keyword)) {
                searchArticleHistoryList.Remove(item: keyword);
            }

            searchArticleHistoryList.Insert(0, item: keyword);
            if (searchArticleHistoryList.Count > _searchArticleHistoryLimit) {
                searchArticleHistoryList.RemoveRange(index: _searchArticleHistoryLimit,
                    searchArticleHistoryList.Count - _searchArticleHistoryLimit);
            }

            var newSearchHistory = JsonConvert.SerializeObject(value: searchArticleHistoryList);
            PlayerPrefs.SetString(_searchArticleHistoryKey + userId, value: newSearchHistory);
            PlayerPrefs.Save();
            return searchArticleHistoryList;
        }

        public static List<string> deleteSearchArticleHistoryList(string keyword, string userId = _visitorId) {
            var searchArticleHistory = PlayerPrefs.GetString(_searchArticleHistoryKey + userId);
            var searchArticleHistoryList = new List<string>();
            if (searchArticleHistory.isNotEmpty()) {
                searchArticleHistoryList = JsonConvert.DeserializeObject<List<string>>(value: searchArticleHistory);
            }

            if (searchArticleHistoryList.Contains(item: keyword)) {
                searchArticleHistoryList.Remove(item: keyword);
            }

            var newSearchHistory = JsonConvert.SerializeObject(value: searchArticleHistoryList);
            PlayerPrefs.SetString(_searchArticleHistoryKey + userId, value: newSearchHistory);
            PlayerPrefs.Save();
            return searchArticleHistoryList;
        }

        public static void deleteAllSearchArticleHistory(string userId = _visitorId) {
            if (PlayerPrefs.HasKey(_searchArticleHistoryKey + userId)) {
                PlayerPrefs.DeleteKey(_searchArticleHistoryKey + userId);
            }
        }

        /// Block Article
        public static List<string> blockArticleList(string userId = null) {
            if (userId == null) {
                return new List<string>();
            }

            var blockArticle = PlayerPrefs.GetString(_blockArticleKey + userId);
            var blockArticleList = new List<string>();
            if (blockArticle.isNotEmpty()) {
                blockArticleList = JsonConvert.DeserializeObject<List<string>>(value: blockArticle);
            }

            return blockArticleList;
        }

        public static List<string> saveBlockArticleList(string articleId, string userId) {
            var blockArticle = PlayerPrefs.GetString(_blockArticleKey + userId);
            var blockArticleList = new List<string>();
            if (blockArticle.isNotEmpty()) {
                blockArticleList = JsonConvert.DeserializeObject<List<string>>(value: blockArticle);
            }

            blockArticleList.Insert(0, item: articleId);
            var newBlockArticle = JsonConvert.SerializeObject(value: blockArticleList);
            PlayerPrefs.SetString(_blockArticleKey + userId, value: newBlockArticle);
            PlayerPrefs.Save();
            return blockArticleList;
        }

        /// Block Question
        public static List<string> blockQuestionList(string userId = null) {
            if (userId == null) {
                return new List<string>();
            }

            var blockQuestion = PlayerPrefs.GetString(_blockQuestionKey + userId);
            var blockQuestionList = new List<string>();
            if (blockQuestion.isNotEmpty()) {
                blockQuestionList = JsonConvert.DeserializeObject<List<string>>(value: blockQuestion);
            }

            return blockQuestionList;
        }

        public static List<string> saveBlockQuestionList(string questionId, string userId) {
            var blockQuestion = PlayerPrefs.GetString(_blockQuestionKey + userId);
            var blockQuestionList = new List<string>();
            if (blockQuestion.isNotEmpty()) {
                blockQuestionList = JsonConvert.DeserializeObject<List<string>>(value: blockQuestion);
            }

            blockQuestionList.Insert(0, item: questionId);
            var newBlockQuestion = JsonConvert.SerializeObject(value: blockQuestionList);
            PlayerPrefs.SetString(_blockQuestionKey + userId, value: newBlockQuestion);
            PlayerPrefs.Save();
            return blockQuestionList;
        }

        /// Block Answer
        public static List<string> blockAnswerList(string userId = null) {
            if (userId == null) {
                return new List<string>();
            }

            var blockAnswer = PlayerPrefs.GetString(_blockAnswerKey + userId);
            var blockAnswerList = new List<string>();
            if (blockAnswer.isNotEmpty()) {
                blockAnswerList = JsonConvert.DeserializeObject<List<string>>(value: blockAnswer);
            }

            return blockAnswerList;
        }

        public static List<string> saveBlockAnswerList(string answerId, string userId) {
            var blockAnswer = PlayerPrefs.GetString(_blockAnswerKey + userId);
            var blockAnswerList = new List<string>();
            if (blockAnswer.isNotEmpty()) {
                blockAnswerList = JsonConvert.DeserializeObject<List<string>>(value: blockAnswer);
            }

            blockAnswerList.Insert(0, item: answerId);
            var newBlockAnswer = JsonConvert.SerializeObject(value: blockAnswerList);
            PlayerPrefs.SetString(_blockAnswerKey + userId, value: newBlockAnswer);
            PlayerPrefs.Save();
            return blockAnswerList;
        }

        /// Block User
        public static HashSet<string> blockUserIdSet(string currentUserId = null) {
            if (currentUserId == null) {
                return new HashSet<string>();
            }

            var blockUserData = PlayerPrefs.GetString(_blockUserKey + currentUserId);
            var blockUserIdSet = new HashSet<string>();
            if (blockUserData.isNotEmpty()) {
                blockUserIdSet = JsonConvert.DeserializeObject<HashSet<string>>(value: blockUserData);
            }

            return blockUserIdSet;
        }

        public static bool isBlockUser(string userId) {
            return blockUserIdSet(currentUserId: UserInfoManager.getUserInfo().userId).Contains(item: userId);
        }

        public static HashSet<string> updateBlockUserId(string blockUserId, string currentUserId, bool remove) {
            var blockUserData = PlayerPrefs.GetString(_blockUserKey + currentUserId);
            var blockUserIdSet = new HashSet<string>();
            if (blockUserData.isNotEmpty()) {
                blockUserIdSet = JsonConvert.DeserializeObject<HashSet<string>>(value: blockUserData);
            }

            if (remove) {
                blockUserIdSet.Remove(item: blockUserId);
            }
            else {
                blockUserIdSet.Add(item: blockUserId);
            }

            var newBlockUserData = JsonConvert.SerializeObject(value: blockUserIdSet);
            PlayerPrefs.SetString(_blockUserKey + currentUserId, value: newBlockUserData);
            PlayerPrefs.Save();
            return blockUserIdSet;
        }
    }
}