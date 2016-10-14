namespace Tailspin.Web.Tests.Area.Survey.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Tailspin.Web.Areas.Survey.Controllers;
    using Tailspin.Web.Areas.Survey.Models;
    using Tailspin.Web.Models;
    using Tailspin.Web.Survey.Shared.Models;
    using Tailspin.Web.Survey.Shared.Stores;

    [TestClass]
    public class SurveysControllerFixture
    {
        [TestMethod]
        public void IndexReturnsEmptyViewName()
        {
            using (var controller = new SurveysController(new Mock<ISurveyStore>().Object, null, null, null, null))
            {
                var result = controller.Index() as ViewResult;

                Assert.AreEqual(string.Empty, result.ViewName);
            }
        }

        [TestMethod]
        public void IndexReturnsMySurveysAsTitleInTheModel()
        {
            using (var controller = new SurveysController(new Mock<ISurveyStore>().Object, null, null, null, null))
            {
                var result = controller.Index() as ViewResult;

                var model = result.ViewData.Model as TenantMasterPageViewData;
                Assert.AreSame("My Surveys", model.Title);
            }
        }

        [TestMethod]
        public void IndexCallsGetAllSurveysByTenantFromSurveyStore()
        {
            var mockSurveyStore = new Mock<ISurveyStore>();

            using (var controller = new SurveysController(mockSurveyStore.Object, null, null, null, null))
            {
                controller.TenantName = "tenant";

                controller.Index();
            }

            mockSurveyStore.Verify(r => r.GetSurveysByTenant(It.Is<string>(actual => "tenant" == actual)), Times.Once());
        }

        [TestMethod]
        public void IndexReturnsTheSurveysForTheTenantInTheModel()
        {
            var mockSurveyStore = new Mock<ISurveyStore>();
            mockSurveyStore.Setup(r => r.GetSurveysByTenant(It.IsAny<string>()))
                .Returns(new List<Survey>() { new Survey() { Title = "title" } });

            using (var controller = new SurveysController(mockSurveyStore.Object, null, null, null, null))
            {
                controller.Tenant = new Tenant() { SubscriptionKind = SubscriptionKind.Standard };

                var result = controller.Index() as ViewResult;

                var model = result.ViewData.Model as TenantPageViewData<IEnumerable<SurveyModel>>;

                Assert.AreEqual(1, model.ContentModel.Count());
                Assert.AreEqual("title", model.ContentModel.First().Title);
            }
        }

        [TestMethod]
        public void NewWhenHttpVerbIsGetReturnsEmptyViewName()
        {
            using (var controller = new SurveysController(new Mock<ISurveyStore>().Object, null, null, null, null))
            {
                controller.Tenant = new Tenant() { SubscriptionKind = SubscriptionKind.Standard };

                var result = controller.New() as ViewResult;

                Assert.AreEqual(string.Empty, result.ViewName);
            }
        }

        [TestMethod]
        public void NewWhenHttpVerbIsGetReturnsNewSurveyAsTitleInTheModel()
        {
            using (var controller = new SurveysController(new Mock<ISurveyStore>().Object, null, null, null, null))
            {
                controller.Tenant = new Tenant() { SubscriptionKind = SubscriptionKind.Standard };

                var result = controller.New() as ViewResult;

                var model = result.ViewData.Model as TenantMasterPageViewData;
                Assert.AreSame("New Survey", model.Title);
            }
        }

        [TestMethod]
        public void NewWhenHttpVerbIsGetReturnsANewSurveyInTheModelWhenCachedSurveyDoesNotExistInTempData()
        {
            using (var controller = new SurveysController(new Mock<ISurveyStore>().Object, null, null, null, null))
            {
                controller.Tenant = new Tenant() { SubscriptionKind = SubscriptionKind.Standard };
                controller.TempData[SurveysController.CachedSurvey] = null;

                var result = controller.New() as ViewResult;

                var model = result.ViewData.Model as TenantPageViewData<SurveyModel>;

                Assert.IsInstanceOfType(model.ContentModel, typeof(SurveyModel));
            }
        }

        [TestMethod]
        public void NewWhenHttpVerbIsGetReturnsTheCachedSurveyInTheModelWhenCachedSurveyExistsInTempData()
        {
            using (var controller = new SurveysController(new Mock<ISurveyStore>().Object, null, null, null, null))
            {
                var survey = new SurveyModel();
                controller.Tenant = new Tenant() { SubscriptionKind = SubscriptionKind.Standard };
                controller.TempData[SurveysController.CachedSurvey] = survey;

                var result = controller.New() as ViewResult;

                var model = result.ViewData.Model as TenantPageViewData<SurveyModel>;

                Assert.AreSame(survey, model.ContentModel);
            }
        }

        [TestMethod]
        public void NewWhenHttpVerbIsPostCallsSaveFromSurveyStoreWithSurveyParameterWhenModelStateIsValid()
        {
            var mockSurveyStore = new Mock<ISurveyStore>();

            var survey = new SurveyModel("slug-name");
            var cachedSurvey = new SurveyModel();
            cachedSurvey.Questions.Add(new Question());

            using (var controller = new SurveysController(mockSurveyStore.Object, null, null, null, null))
            {
                controller.TenantName = "Tenant";
                controller.Tenant = new Tenant() { SubscriptionKind = SubscriptionKind.Standard };
                controller.TempData[SurveysController.CachedSurvey] = cachedSurvey;

                controller.New(survey);
            }

            mockSurveyStore.Verify(r => r.SaveSurvey(survey), Times.Once());
        }

        [TestMethod]
        public void NewWhenHttpVerbIsPostSavedTenantNameIsSameAsControllerWhenModelStateIsValid()
        {
            var mockSurveyStore = new Mock<ISurveyStore>();

            var cachedSurvey = new SurveyModel();
            cachedSurvey.Questions.Add(new Question());

            using (var controller = new SurveysController(mockSurveyStore.Object, null, null, null, null))
            {
                controller.TenantName = "Tenant";
                controller.Tenant = new Tenant() { SubscriptionKind = SubscriptionKind.Standard };
                controller.TempData[SurveysController.CachedSurvey] = cachedSurvey;

                controller.New(new SurveyModel());
            }

            mockSurveyStore.Verify(r => r.SaveSurvey(It.Is<Survey>(s => s.Tenant == "Tenant")), Times.Once());
        }

        [TestMethod]
        public void NewWhenHttpVerbIsPostCopiesQuestionsFromCachedSurveyToSurveyWhenCallingSaveFromSurveyStoreWhenModelStateIsValid()
        {
            var mockSurveyStore = new Mock<ISurveyStore>();

            var survey = new SurveyModel("slug-name");
            var questionsToBeCopied = new List<Question>();
            questionsToBeCopied.Add(new Question());
            var cachedSurvey = new SurveyModel { Questions = questionsToBeCopied };

            using (var controller = new SurveysController(mockSurveyStore.Object, null, null, null, null))
            {
                controller.TenantName = "Tenant";
                controller.Tenant = new Tenant() { SubscriptionKind = SubscriptionKind.Standard };
                controller.TempData[SurveysController.CachedSurvey] = cachedSurvey;

                controller.New(survey);
            }

            mockSurveyStore.Verify(r => r.SaveSurvey(It.Is<Survey>(actual => questionsToBeCopied == actual.Questions)));
        }

        [TestMethod]
        public void NewWhenHttpVerbIsPostCleansCachedSurveyWhenModelStateIsValid()
        {
            var cachedSurvey = new SurveyModel();
            cachedSurvey.Questions.Add(new Question());

            using (var controller = new SurveysController(new Mock<ISurveyStore>().Object, null, null, null, null))
            {
                controller.TenantName = "Tenant";
                controller.Tenant = new Tenant() { SubscriptionKind = SubscriptionKind.Standard };
                controller.TempData[SurveysController.CachedSurvey] = cachedSurvey;

                controller.New(new SurveyModel());

                Assert.IsNull(controller.TempData[SurveysController.CachedSurvey]);
            }
        }

        [TestMethod]
        public void NewWhenHttpVerbIsPostReturnsRedirectToMySurveysWhenModelStateIsValid()
        {
            var cachedSurvey = new SurveyModel();
            cachedSurvey.Questions.Add(new Question());

            using (var controller = new SurveysController(new Mock<ISurveyStore>().Object, null, null, null, null))
            {
                controller.TenantName = "Tenant";
                controller.Tenant = new Tenant() { SubscriptionKind = SubscriptionKind.Standard };
                controller.TempData[SurveysController.CachedSurvey] = cachedSurvey;

                var result = controller.New(new SurveyModel()) as RedirectToRouteResult;

                Assert.AreEqual("Index", result.RouteValues["action"]);
                Assert.AreEqual(null, result.RouteValues["controller"]);
            }
        }

        [TestMethod]
        public void NewWhenHttpVerbIsPostReturnsRedirectToTheNewActionWhenCachedSurveyIsNull()
        {
            using (var controller = new SurveysController(new Mock<ISurveyStore>().Object, null, null, null, null))
            {
                controller.TempData[SurveysController.CachedSurvey] = null;

                var result = controller.New(new SurveyModel()) as RedirectToRouteResult;

                Assert.AreEqual("New", result.RouteValues["action"]);
                Assert.AreEqual(null, result.RouteValues["controller"]);
            }
        }

        [TestMethod]
        public void NewWhenHttpVerbIsPostReturnsEmptyViewNameWhenModelStateIsNotValid()
        {
            using (var controller = new SurveysController(new Mock<ISurveyStore>().Object, null, null, null, null))
            {
                controller.ModelState.AddModelError("error for test", "invalid model state");
                controller.TempData[SurveysController.CachedSurvey] = new SurveyModel();

                var result = controller.New(new SurveyModel()) as ViewResult;

                Assert.AreEqual(string.Empty, result.ViewName);
            }
        }

        [TestMethod]
        public void NewWhenHttpVerbIsPostReturnsTheSameModelWhenModelStateIsNotValid()
        {
            var survey = new SurveyModel();

            using (var controller = new SurveysController(new Mock<ISurveyStore>().Object, null, null, null, null))
            {
                controller.ModelState.AddModelError("error for test", "invalid model state");
                controller.TempData[SurveysController.CachedSurvey] = new SurveyModel();

                var result = controller.New(survey) as ViewResult;

                var model = result.ViewData.Model as TenantPageViewData<SurveyModel>;

                Assert.AreSame(survey, model.ContentModel);
            }
        }

        [TestMethod]
        public void NewWhenHttpVerbIsPostReturnsTitleInTheModelWhenModelStateIsNotValid()
        {
            var survey = new SurveyModel();

            using (var controller = new SurveysController(new Mock<ISurveyStore>().Object, null, null, null, null))
            {
                controller.ModelState.AddModelError("error for test", "invalid model state");
                controller.TempData[SurveysController.CachedSurvey] = new SurveyModel();

                var result = controller.New(survey) as ViewResult;

                var model = result.ViewData.Model as TenantMasterPageViewData;
                Assert.AreSame("New Survey", model.Title);
            }
        }

        [TestMethod]
        public void NewWhenHttpVerbIsPostSavesCachedSurveyInTempDataWhenModelStateIsNotValid()
        {
            var cachedSurvey = new SurveyModel();

            using (var controller = new SurveysController(new Mock<ISurveyStore>().Object, null, null, null, null))
            {
                controller.ModelState.AddModelError("error for test", "invalid model state");
                controller.TempData[SurveysController.CachedSurvey] = cachedSurvey;

                var result = controller.New(new SurveyModel()) as ViewResult;

                var model = result.ViewData.Model as TenantPageViewData<Survey>;
                Assert.AreSame(cachedSurvey, controller.TempData[SurveysController.CachedSurvey]);
            }
        }

        [TestMethod]
        public void NewWhenHttpVerbIsPostCopiesQuestionsFromCachedSurveyToSurveyWhenModelStateIsNotValid()
        {
            var mockSurveyStore = new Mock<ISurveyStore>();

            var questionsToBeCopied = new List<Question>();
            questionsToBeCopied.Add(new Question());
            var cachedSurvey = new SurveyModel { Questions = questionsToBeCopied };

            using (var controller = new SurveysController(mockSurveyStore.Object, null, null, null, null))
            {
                controller.ModelState.AddModelError("error for test", "invalid model state");
                controller.TempData[SurveysController.CachedSurvey] = cachedSurvey;

                var result = controller.New(new SurveyModel()) as ViewResult;

                var model = result.ViewData.Model as TenantPageViewData<SurveyModel>;

                Assert.AreSame(model.ContentModel.Questions, questionsToBeCopied);
            }
        }

        [TestMethod]
        public void NewWhenHttpVerbIsPostReturnsErrorInModelStateWhenCachedSurveyHasNoQuestions()
        {
            var cachedSurvey = new SurveyModel { Questions = new List<Question>() };

            using (var controller = new SurveysController(new Mock<ISurveyStore>().Object, null, null, null, null))
            {
                controller.TempData[SurveysController.CachedSurvey] = cachedSurvey;

                var result = controller.New(new SurveyModel()) as ViewResult;

                Assert.IsTrue(controller.ModelState.Keys.Contains("ContentModel.Questions"));
            }
        }

        [TestMethod]
        public void NewWhenHttpVerbIsPostReturnsTheSameModelWhenCachedSurveyHasNoQuestions()
        {
            var cachedSurvey = new SurveyModel { Questions = new List<Question>() };
            var survey = new SurveyModel();

            using (var controller = new SurveysController(new Mock<ISurveyStore>().Object, null, null, null, null))
            {
                controller.TempData[SurveysController.CachedSurvey] = cachedSurvey;

                var result = controller.New(survey) as ViewResult;
                var model = result.ViewData.Model as TenantPageViewData<SurveyModel>;

                Assert.AreSame(survey, model.ContentModel);
            }
        }

        [TestMethod]
        public void NewWhenHttpVerbIsPostReturnsNewSurveyAsTitleInTheModelWhenCachedSurveyHasNoQuestions()
        {
            var cachedSurvey = new SurveyModel { Questions = new List<Question>() };

            using (var controller = new SurveysController(new Mock<ISurveyStore>().Object, null, null, null, null))
            {
                controller.TempData[SurveysController.CachedSurvey] = cachedSurvey;

                var result = controller.New(new SurveyModel()) as ViewResult;
                var model = result.ViewData.Model as TenantMasterPageViewData;

                Assert.AreSame("New Survey", model.Title);
            }
        }

        [TestMethod]
        public void NewWhenHttpVerbIsPostSavesCachedSurveyInTempDataWhenCachedSurveyHasNoQuestions()
        {
            var cachedSurvey = new SurveyModel { Questions = new List<Question>() };

            using (var controller = new SurveysController(new Mock<ISurveyStore>().Object, null, null, null, null))
            {
                controller.TempData[SurveysController.CachedSurvey] = cachedSurvey;

                var result = controller.New(new SurveyModel()) as ViewResult;

                var model = result.ViewData.Model as TenantPageViewData<Survey>;
                Assert.AreSame(cachedSurvey, controller.TempData[SurveysController.CachedSurvey]);
            }
        }

        [TestMethod]
        public void NewQuestionReturnsEmptyViewName()
        {
            using (var controller = new SurveysController(null, null, null, null, null))
            {
                controller.TempData[SurveysController.CachedSurvey] = new SurveyModel();

                var result = controller.NewQuestion(new SurveyModel()) as ViewResult;

                Assert.AreEqual(string.Empty, result.ViewName);
            }
        }

        [TestMethod]
        public void NewQuestionReturnsNewQuestionAsTitleInTheModel()
        {
            using (var controller = new SurveysController(null, null, null, null, null))
            {
                controller.TempData[SurveysController.CachedSurvey] = new SurveyModel();

                var result = controller.NewQuestion(new SurveyModel()) as ViewResult;

                var model = result.ViewData.Model as TenantMasterPageViewData;
                Assert.AreSame("New Question", model.Title);
            }
        }

        [TestMethod]
        public void NewQuestionReturnsNewQuestionInTheModel()
        {
            using (var controller = new SurveysController(null, null, null, null, null))
            {
                controller.TempData[SurveysController.CachedSurvey] = new SurveyModel();

                var result = controller.NewQuestion(new SurveyModel()) as ViewResult;

                var model = result.ViewData.Model as TenantPageViewData<Question>;
                Assert.IsInstanceOfType(model.ContentModel, typeof(Question));
            }
        }

        [TestMethod]
        public void NewQuestionRedirectToTheNewActionWhenCachedSurveyIsNull()
        {
            using (var controller = new SurveysController(new Mock<ISurveyStore>().Object, null, null, null, null))
            {
                controller.TempData[SurveysController.CachedSurvey] = null;

                var result = controller.NewQuestion(new SurveyModel()) as RedirectToRouteResult;

                Assert.AreEqual("New", result.RouteValues["action"]);
                Assert.AreEqual(null, result.RouteValues["controller"]);
            }
        }

        [TestMethod]
        public void NewQuestionCopiesSurveyTitleToCachedSurveyThatIsReturnedInViewData()
        {
            var survey = new SurveyModel { Title = "title" };

            using (var controller = new SurveysController(null, null, null, null, null))
            {
                controller.TempData[SurveysController.CachedSurvey] = new SurveyModel();

                var result = controller.NewQuestion(survey) as ViewResult;

                var cachedSurvey = result.TempData[SurveysController.CachedSurvey] as SurveyModel;
                Assert.AreSame(survey.Title, cachedSurvey.Title);
            }
        }

        [TestMethod]
        public void AddQuestionReturnsRedirectToNewSurveyWhenModelIsValid()
        {
            using (var controller = new SurveysController(null, null, null, null, null))
            {
                controller.TempData[SurveysController.CachedSurvey] = new SurveyModel();

                var result = controller.AddQuestion(new Question()) as RedirectToRouteResult;

                Assert.AreEqual("New", result.RouteValues["action"]);
                Assert.AreEqual(null, result.RouteValues["controller"]);
            }
        }

        [TestMethod]
        public void AddQuestionAddsTheNewQuestionToTheCachedSurveyWhenModelIsValid()
        {
            var cachedSurvey = new SurveyModel();
            cachedSurvey.Questions.Add(new Question());
            var question = new Question();

            using (var controller = new SurveysController(null, null, null, null, null))
            {
                controller.TempData[SurveysController.CachedSurvey] = cachedSurvey;
                controller.AddQuestion(question);

                var actualQuestions = (controller.TempData[SurveysController.CachedSurvey] as SurveyModel).Questions;

                Assert.AreEqual(2, actualQuestions.Count);
                Assert.IsTrue(actualQuestions.Contains(question));
            }
        }

        [TestMethod]
        public void AddQuestionReplacesCarriageReturnsInPossibleAnswersWhenModelIsValid()
        {
            using (var controller = new SurveysController(null, null, null, null, null))
            {
                var question = new Question { PossibleAnswers = "possible answers\r\n" };
                controller.TempData[SurveysController.CachedSurvey] = new SurveyModel();

                controller.AddQuestion(question);

                var cachedSurvey = controller.TempData[SurveysController.CachedSurvey] as SurveyModel;

                var actualQuestion = cachedSurvey.Questions.Single(q => q == question);
                Assert.AreEqual("possible answers\n", actualQuestion.PossibleAnswers);
            }
        }

        [TestMethod]
        public void AddQuestionReturnsNewQuestionViewWhenModelIsNotValid()
        {
            using (var controller = new SurveysController(null, null, null, null, null))
            {
                controller.ModelState.AddModelError("error for test", "invalid model state");

                var result = controller.AddQuestion(null) as ViewResult;

                Assert.AreSame("NewQuestion", result.ViewName);
            }
        }

        [TestMethod]
        public void AddQuestionReturnsQuestionAsModelWhenModelIsNotValid()
        {
            using (var controller = new SurveysController(null, null, null, null, null))
            {
                controller.TempData[SurveysController.CachedSurvey] = new SurveyModel();
                controller.ModelState.AddModelError("error for test", "invalid model state");
                var question = new Question();

                var result = controller.AddQuestion(question) as ViewResult;

                var model = result.ViewData.Model as TenantPageViewData<Question>;

                Assert.AreSame(question, model.ContentModel);
            }
        }

        [TestMethod]
        public void AddQuestionReturnsNewQuestionAsModelWhenModelIsNotValidAndQuestionIsNull()
        {
            using (var controller = new SurveysController(null, null, null, null, null))
            {
                controller.TempData[SurveysController.CachedSurvey] = new SurveyModel();
                controller.ModelState.AddModelError("error for test", "invalid model state");

                var result = controller.AddQuestion(null) as ViewResult;

                var model = result.ViewData.Model as TenantPageViewData<Question>;

                Assert.IsInstanceOfType(model.ContentModel, typeof(Question));
            }
        }

        [TestMethod]
        public void AddQuestionCopiesCachedSurveyToTempDataWhenModelIsNotValid()
        {
            using (var controller = new SurveysController(null, null, null, null, null))
            {
                controller.ModelState.AddModelError("error for test", "invalid model state");
                var cachedSurvey = new SurveyModel();
                controller.TempData[SurveysController.CachedSurvey] = cachedSurvey;

                var result = controller.AddQuestion(null) as ViewResult;

                var cachedSurveyReturnedInTempData = result.TempData[SurveysController.CachedSurvey] as SurveyModel;

                Assert.AreSame(cachedSurvey, cachedSurveyReturnedInTempData);
            }
        }

        [TestMethod]
        public void AddQuestionReturnsNewQuestionAsTitleInTheModelWhenModelIsNotValid()
        {
            using (var controller = new SurveysController(null, null, null, null, null))
            {
                controller.ModelState.AddModelError("error for test", "invalid model state");

                var result = controller.AddQuestion(null) as ViewResult;

                var model = result.ViewData.Model as TenantMasterPageViewData;
                Assert.AreSame("New Question", model.Title);
            }
        }

        [TestMethod]
        public void DeleteCallsDeleteSurveyByTenantAndSlugNameFromSurveyStore()
        {
            var mockSurveyStore = new Mock<ISurveyStore>();

            using (var controller = new SurveysController(mockSurveyStore.Object, new Mock<ISurveyAnswerStore>().Object, new Mock<ISurveyAnswersSummaryStore>().Object, null, null))
            {
                controller.Delete("tenant", "survey-slug");
            }

            mockSurveyStore.Verify(
                r => r.DeleteSurveyByTenantAndSlugName(
                    It.Is<string>(t => "tenant" == t),
                    It.Is<string>(s => "survey-slug" == s)),
                Times.Once());
        }

        [TestMethod]
        public void DeleteCallsDeleteSurveyAnswersStore()
        {
            var mockSurveyAnswerStore = new Mock<ISurveyAnswerStore>();

            using (var controller = new SurveysController(new Mock<ISurveyStore>().Object, mockSurveyAnswerStore.Object, new Mock<ISurveyAnswersSummaryStore>().Object, null, null))
            {
                controller.Delete("tenant", "survey-slug");
            }

            mockSurveyAnswerStore.Verify(r => r.DeleteSurveyAnswers("tenant", "survey-slug"), Times.Once());
        }

        [TestMethod]
        public void DeleteCallsDeleteSurveyAnswersSummariesStore()
        {
            var mockSurveyAnswersSummaryStore = new Mock<ISurveyAnswersSummaryStore>();

            using (var controller = new SurveysController(new Mock<ISurveyStore>().Object, new Mock<ISurveyAnswerStore>().Object, mockSurveyAnswersSummaryStore.Object, null, null))
            {
                controller.Delete("tenant", "survey-slug");
            }

            mockSurveyAnswersSummaryStore.Verify(r => r.DeleteSurveyAnswersSummary("tenant", "survey-slug"), Times.Once());
        }

        [TestMethod]
        public void DeleteReturnsRedirectToMySurveys()
        {
            using (var controller = new SurveysController(new Mock<ISurveyStore>().Object, new Mock<ISurveyAnswerStore>().Object, new Mock<ISurveyAnswersSummaryStore>().Object, null, null))
            {
                var result = controller.Delete(string.Empty, string.Empty) as RedirectToRouteResult;

                Assert.AreEqual("Index", result.RouteValues["action"]);
                Assert.AreEqual(null, result.RouteValues["controller"]);
            }
        }

        [TestMethod]
        public void BrowseResponsesReturnSlugNameAsTheTitle()
        {
            var mockSurveyAnswerStore = new Mock<ISurveyAnswerStore>();
            mockSurveyAnswerStore.Setup(r => r.GetSurveyAnswerBrowsingContext(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                                      .Returns(new SurveyAnswerBrowsingContext { PreviousId = string.Empty, NextId = string.Empty });

            using (var controller = new SurveysController(null, mockSurveyAnswerStore.Object, null, null, null))
            {
                var result = controller.BrowseResponses(string.Empty, "slug-name", string.Empty) as ViewResult;

                var model = result.ViewData.Model as TenantMasterPageViewData;
                Assert.AreSame("slug-name", model.Title);
            }
        }

        [TestMethod]
        public void BrowseResponsesGetsTheAnswerFromTheStoreWhenAnswerIdIsNotEmpty()
        {
            var mockSurveyAnswerStore = new Mock<ISurveyAnswerStore>();
            mockSurveyAnswerStore.Setup(r => r.GetSurveyAnswerBrowsingContext(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                                      .Returns(new SurveyAnswerBrowsingContext { PreviousId = string.Empty, NextId = string.Empty });

            using (var controller = new SurveysController(null, mockSurveyAnswerStore.Object, null, null, null))
            {
                controller.BrowseResponses("tenant", "survey-slug", "answer id");
            }

            mockSurveyAnswerStore.Verify(r => r.GetSurveyAnswer("tenant", "survey-slug", "answer id"));
        }

        [TestMethod]
        public void BrowseResponsesGetsTheFirstAnswerIdFromTheStoreWhenAnswerIdIsEmpty()
        {
            var mockSurveyAnswerStore = new Mock<ISurveyAnswerStore>();
            mockSurveyAnswerStore.Setup(r => r.GetSurveyAnswerBrowsingContext(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                                      .Returns(new SurveyAnswerBrowsingContext { PreviousId = string.Empty, NextId = string.Empty });

            using (var controller = new SurveysController(null, mockSurveyAnswerStore.Object, null, null, null))
            {
                controller.BrowseResponses("tenant", "survey-slug", string.Empty);
            }

            mockSurveyAnswerStore.Verify(r => r.GetFirstSurveyAnswerId("tenant", "survey-slug"), Times.Once());
        }

        [TestMethod]
        public void BrowseResponsesGetsSurveyAnswerWithTheIdReturnedFromTheStoreWhenAnswerIdIsEmpty()
        {
            var mockSurveyAnswerStore = new Mock<ISurveyAnswerStore>();
            mockSurveyAnswerStore.Setup(r => r.GetSurveyAnswerBrowsingContext(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                                      .Returns(new SurveyAnswerBrowsingContext { PreviousId = string.Empty, NextId = string.Empty });
            mockSurveyAnswerStore.Setup(r => r.GetFirstSurveyAnswerId("tenant", "survey-slug"))
                                      .Returns("id");

            using (var controller = new SurveysController(null, mockSurveyAnswerStore.Object, null, null, null))
            {
                controller.BrowseResponses("tenant", "survey-slug", string.Empty);
            }

            mockSurveyAnswerStore.Verify(r => r.GetSurveyAnswer("tenant", "survey-slug", "id"));
        }

        [TestMethod]
        public void BrowseResponsesSetsTheAnswerFromTheStoreInTheModel()
        {
            var mockSurveyAnswerStore = new Mock<ISurveyAnswerStore>();
            var surveyAnswer = new SurveyAnswer();
            mockSurveyAnswerStore.Setup(r => r.GetSurveyAnswerBrowsingContext(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                                      .Returns(new SurveyAnswerBrowsingContext { PreviousId = string.Empty, NextId = string.Empty });
            mockSurveyAnswerStore.Setup(r => r.GetSurveyAnswer("tenant", "survey-slug", "answer id"))
                                      .Returns(surveyAnswer);

            using (var controller = new SurveysController(null, mockSurveyAnswerStore.Object, null, null, null))
            {
                var result = controller.BrowseResponses("tenant", "survey-slug", "answer id") as ViewResult;

                var model = result.ViewData.Model as TenantPageViewData<BrowseResponseModel>;
                Assert.AreSame(surveyAnswer, model.ContentModel.SurveyAnswer);
            }
        }

        [TestMethod]
        public void BrowseResponsesCallsGetSurveyAnswerBrowsingContextFromStore()
        {
            var mockSurveyAnswerStore = new Mock<ISurveyAnswerStore>();
            mockSurveyAnswerStore.Setup(r => r.GetSurveyAnswerBrowsingContext(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                                      .Returns(new SurveyAnswerBrowsingContext { PreviousId = string.Empty, NextId = string.Empty });

            using (var controller = new SurveysController(null, mockSurveyAnswerStore.Object, null, null, null))
            {
                controller.BrowseResponses("tenant", "survey-slug", "answer id");
            }

            mockSurveyAnswerStore.Verify(
                r => r.GetSurveyAnswerBrowsingContext("tenant", "survey-slug", "answer id"),
                Times.Once());
        }

        [TestMethod]
        public void BrowseResponsesSetsPreviousAndNextIdsInModel()
        {
            var mockSurveyAnswerStore = new Mock<ISurveyAnswerStore>();
            mockSurveyAnswerStore.Setup(r => r.GetSurveyAnswerBrowsingContext(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                                      .Returns(new SurveyAnswerBrowsingContext { PreviousId = "PreviousId", NextId = "NextId" });

            using (var controller = new SurveysController(null, mockSurveyAnswerStore.Object, null, null, null))
            {
                var result = controller.BrowseResponses("tenant", "survey-slug", "answer id") as ViewResult;

                var model = result.ViewData.Model as TenantPageViewData<BrowseResponseModel>;
                Assert.AreEqual("PreviousId", model.ContentModel.PreviousAnswerId);
                Assert.AreEqual("NextId", model.ContentModel.NextAnswerId);
            }
        }

        [TestMethod]
        public void AnalyzeReturnSlugNameAsTheTitle()
        {
            using (var controller = new SurveysController(null, null, new Mock<ISurveyAnswersSummaryStore>().Object, null, null))
            {
                var result = controller.Analyze(string.Empty, "slug-name") as ViewResult;

                var model = result.ViewData.Model as TenantMasterPageViewData;
                Assert.AreSame("slug-name", model.Title);
            }
        }

        [TestMethod]
        public void AnalyzeGetsTheSummaryFromTheStore()
        {
            var mockSurveyAnswersSummaryStore = new Mock<ISurveyAnswersSummaryStore>();

            using (var controller = new SurveysController(null, null, mockSurveyAnswersSummaryStore.Object, null, null))
            {
                controller.Analyze("tenant", "slug-name");
            }

            mockSurveyAnswersSummaryStore.Verify(r => r.GetSurveyAnswersSummary("tenant", "slug-name"), Times.Once());
        }

        [TestMethod]
        public void AnalyzeReturnsTheSummaryGetFromTheStoreInTheModel()
        {
            var mockSurveyAnswersSummaryStore = new Mock<ISurveyAnswersSummaryStore>();
            var surveyAnswersSummary = new SurveyAnswersSummary();
            mockSurveyAnswersSummaryStore.Setup(r => r.GetSurveyAnswersSummary(It.IsAny<string>(), It.IsAny<string>()))
                                              .Returns(surveyAnswersSummary);

            using (var controller = new SurveysController(null, null, mockSurveyAnswersSummaryStore.Object, null, null))
            {
                var result = controller.Analyze(string.Empty, string.Empty) as ViewResult;

                var model = result.ViewData.Model as TenantPageViewData<SurveyAnswersSummary>;
                Assert.AreSame(surveyAnswersSummary, model.ContentModel);
            }
        }

        [TestMethod]
        public void ExportGetsTheTenantInformationAndPutsInModel()
        {
            var tenant = new Tenant();

            var mockTenantStore = new Mock<ITenantStore>();
            var mockSurveyAnswerStore = new Mock<ISurveyAnswerStore>();
            mockTenantStore.Setup(r => r.GetTenant(It.IsAny<string>())).Returns(tenant);
            mockSurveyAnswerStore.Setup(r => r.GetFirstSurveyAnswerId(It.IsAny<string>(), It.IsAny<string>())).Returns(string.Empty);

            using (var controller = new SurveysController(null, mockSurveyAnswerStore.Object, null, mockTenantStore.Object, null))
            {
                controller.Tenant = tenant;

                var result = controller.ExportResponses(string.Empty) as ViewResult;

                var model = result.ViewData.Model as TenantPageViewData<ExportResponseModel>;

                Assert.AreSame(tenant, model.ContentModel.Tenant);
            }
        }

        [TestMethod]
        public void ExportDeterminesIfThereAreResponsesToExport()
        {
            var mockTenantStore = new Mock<ITenantStore>();
            var mockSurveyAnswerStore = new Mock<ISurveyAnswerStore>();
            var tenant = new Tenant();
            mockTenantStore.Setup(r => r.GetTenant(It.IsAny<string>())).Returns(tenant);
            mockSurveyAnswerStore.Setup(r => r.GetFirstSurveyAnswerId(It.IsAny<string>(), It.IsAny<string>())).Returns(string.Empty);

            using (var controller = new SurveysController(null, mockSurveyAnswerStore.Object, null, mockTenantStore.Object, null))
            {
                var result = controller.ExportResponses(string.Empty) as ViewResult;

                var model = result.ViewData.Model as TenantPageViewData<ExportResponseModel>;
                Assert.AreEqual(false, model.ContentModel.HasResponses);
            }
        }

        [TestMethod]
        public void ExportPostCallsTransferPostWithTenantAndSlugName()
        {
            var mockSurveyTransferStore = new Mock<ISurveyTransferStore>();

            using (var controller = new SurveysController(null, null, null, null, mockSurveyTransferStore.Object))
            {
                controller.ExportResponses("tenant", "slugName");
            }

            mockSurveyTransferStore.Verify(r => r.Transfer("tenant", "slugName"), Times.Once());
        }

        [TestMethod]
        public void ExportPostRedirectsToBrowseAction()
        {
            var mockSurveyTransferStore = new Mock<ISurveyTransferStore>();

            using (var controller = new SurveysController(null, null, null, null, mockSurveyTransferStore.Object))
            {
                var result = controller.ExportResponses("tenant", "slugName") as RedirectToRouteResult;

                Assert.AreEqual(result.RouteValues["action"], "BrowseResponses");
            }
        }
    }
}