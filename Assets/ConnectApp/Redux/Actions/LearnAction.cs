using System.Collections.Generic;
using ConnectApp.Api;
using ConnectApp.Models.Api;
using ConnectApp.Models.Model;
using ConnectApp.Models.State;
using Unity.UIWidgets.Redux;

namespace ConnectApp.redux.actions {
    public class FetchLearnCourseListSuccessAction : BaseAction {
        public List<LearnCourse> courses;
        public bool hasMore;
        public int currentPage;
    }
    
    public static partial class CActions {
        public static object fetchLearnCourseList(int page) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return LearnApi.FetchLearnCourseList(page: page)
                    .then(data => {
                        if (!(data is FetchLearnCourseListResponse learnCourseListResponse)) {
                            return;
                        }
                        dispatcher.dispatch(new FetchLearnCourseListSuccessAction {
                            courses = learnCourseListResponse.results,
                            hasMore = learnCourseListResponse.hasMore,
                            currentPage = page
                        });
                    });
            });
        }
    }
}