using UnityEngine;
using System.Threading.Tasks;
using Firebase.Auth;
using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Firebase;
using Firebase.Unity.Editor;
using Firebase.Database;
using Tests.Util;

namespace Tests
{
    public class Constants
    {
        public static readonly string TEST_EMAIL = "TestEmail@FakeDomain.com";
        public static readonly string TEST_PASSWORD = "NotARealPassword";

        public static readonly string TEST_EMAIL_OTHER = "OtherTestEmail@FakeDomain.com";
        public static readonly string TEST_PASSWORD_OTHER = "DefinitelyNotARealPassword";

        public static readonly string TEST_RECIPE_NAME = "Test Recipe";
        public static readonly string TEST_RECIPE_CALORIES = "7";
        public static readonly string TEST_RECIPE_MINUTES = "15";
        public static readonly ReadOnlyCollection<string> TEST_RECIPE_INGREDIENTS = Array.AsReadOnly(
            new[] {
                "1/2 test ingredient"
            }
            );
        public static readonly ReadOnlyCollection<string> TEST_RECIPE_DIRECTIONS = Array.AsReadOnly(
            new[] {
                "Test direction"
            }
            );
        public static readonly ReadOnlyCollection<string> TEST_RECIPE_TAGS = Array.AsReadOnly(
            new[] {
                "dairyFree"
            }
            );

        public static Recipe[] RECIPES = new[]
            {
                new Recipe("Comforting Chicken Casserole with Green Beans",
                    TestImages.BlackTexturePath,
                    0,
                    45,
                    new List<string>(){"glutenFree", "dairyFree"},
                    new List<Ingredient>()
                    {
                        new Ingredient("Lime", "1"),
                        new Ingredient("Lime Zest", "3 Tablespoons"),
                        new Ingredient("Unsalted Peanuts", "2 Tablespoons")
                    },
                    new List<string>()
                    {
                        "If using the Grilled Chicken Chunks, you may want to chop them into bite sized pieces",
                        "Place chicken, slaw, red pepper, cucumber, cilantro, green onion and peanuts into a bowl",
                        "Drizzle with dressing and sriracha",
                        "Toss together using tongs",
                        "Put into containers to take on the go or inside a large tortilla to make a wrap",
                        "Serve with lime wedges"
                    }, new List<Review>(), 0)
            };
        public static Review[] REVIEWS = new[]
        {
            new Review("example_content", DateTime.Now, "preset_data")
        };
    }

    public class TestImages
    {
        public static readonly string BlackTexturePath;
        public static readonly Texture2D BlackTexture = Texture2D.blackTexture;
        static TestImages()
        {
            var png = BlackTexture.EncodeToPNG();
            string path = Application.temporaryCachePath + "/blackTexture2Dtemp.png";
            if (!System.IO.File.Exists(path))
                System.IO.File.WriteAllBytes(path, png);
            BlackTexturePath = path;
        }

        public static void CameraPickBlackTexture()
        {
            CameraManager camera = UnityEngine.Object.FindObjectOfType<CameraManager>();
            Assert.NotNull(camera);
            camera.OnImagePicked(BlackTexture, BlackTexturePath);
        }
    }



    namespace Common
    {
        public class Database
        {

            public static IEnumerator Setup()
            {
                DatabaseManager.Endpoint = "https://regen-66cf8-automated-tests.firebaseio.com/";
                FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(DatabaseManager.Endpoint);
                var recipe0 = JsonUtility.ToJson(Constants.RECIPES[0]);
                yield return FirebaseDatabase.DefaultInstance.RootReference.SetRawJsonValueAsync("{}").WithFailure<FirebaseException>(e => Debug.LogException(e));
                
                var ref0 = FirebaseDatabase.DefaultInstance.RootReference.Child("recipes").Child("0");
                yield return ref0.SetRawJsonValueAsync(recipe0);

                var reviewRef = FirebaseDatabase.DefaultInstance.RootReference.Child("reviews").Child("0");
                foreach (var r in Constants.REVIEWS)
                {
                    var ra = reviewRef.Child(r.userId);
                    yield return ra.Child("content").SetValueAsync(r.content);
                    yield return ra.Child("timestamp").SetValueAsync(r.timestamp.ToBinary());
                }
                 
            }
        }

        public class Auth
        {
            public static IEnumerator EnsureTestUserDoesNotExist()
            {
                return EnsureTestUserDoesNotExist(Constants.TEST_EMAIL, Constants.TEST_PASSWORD).AsIEnumerator();
            }

            public static IEnumerator EnsureOtherTestUserDoesNotExist()
            {
                return EnsureTestUserDoesNotExist(Constants.TEST_EMAIL_OTHER, Constants.TEST_PASSWORD_OTHER).AsIEnumerator();
            }

            public static async Task EnsureTestUserDoesNotExist(string email, string pass)
            {
                string debugPrefix = "EnsureTestUserDoesNotExist: ";
                Debug.Log(debugPrefix + "Begin");
                // remove existing test user if any
                Task<FirebaseUser> loginTask = FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(email, pass);
                FirebaseUser user;
                try
                {
                    user = await loginTask;
                }
                catch (AggregateException)
                {
                    Debug.Log(debugPrefix + "Finish");
                    return;
                }
                Debug.Log(debugPrefix + "Test User Found: " + user.Email);
                Task deleteTask = user.DeleteAsync();
                await deleteTask;
                if (!deleteTask.IsFaulted)
                {
                    Debug.Log(debugPrefix + "Deleted Test User Successfully");
                }
                else
                {
                    Debug.LogError(debugPrefix + "Failed to Delete Test User: " + deleteTask.Exception.GetBaseException());
                }
                FirebaseAuth.DefaultInstance.SignOut();
                Debug.Log(debugPrefix + "Finish");
                return;
            }

            public static IEnumerator EnsureTestUserExists()
            {
                return EnsureTestUserExists(false, Constants.TEST_EMAIL, Constants.TEST_PASSWORD).AsIEnumerator();
            }

            public static IEnumerator EnsureOtherTestUserExists()
            {
                return EnsureTestUserExists(false, Constants.TEST_EMAIL_OTHER, Constants.TEST_PASSWORD_OTHER).AsIEnumerator();
            }

            public static async Task EnsureTestUserExists(bool staySignedIn, string email, string pass)
            {
                string debugPrefix = "EnsureTestUserExists: ";
                Debug.Log(debugPrefix + "Begin");
                // find existing test user if any
                FirebaseAuth firebase = FirebaseAuth.DefaultInstance;
                Task<FirebaseUser> loginTask = firebase.SignInWithEmailAndPasswordAsync(email, pass);
                FirebaseUser user;
                try
                {
                    user = await loginTask;
                }
                catch (AggregateException)
                {
                    // user does not exist so we create it
                    Task<FirebaseUser> createUserTask = firebase.CreateUserWithEmailAndPasswordAsync(Constants.TEST_EMAIL, Constants.TEST_PASSWORD);
                    try
                    {
                        user = await createUserTask;
                    }
                    catch (AggregateException)
                    {
                        // failed to create user
                        Debug.LogError(debugPrefix + "Failed to Create Test User: " + createUserTask.Exception.GetBaseException());
                        return;
                    }
                }


                if (!staySignedIn)
                {
                    Task nextAuthEvent = firebase.WaitForNextEvent("StateChanged");
                    firebase.SignOut();
                    await nextAuthEvent;
                }



                Debug.Log(debugPrefix + "Finish");
                return;
            }

            public static IEnumerator EnsureUserExists(bool staySignedIn, string email, string pass)
            {
                string debugPrefix = "EnsureTestUserExists: ";
                Debug.Log(debugPrefix + "Begin");
                // find existing test user if any
                FirebaseAuth firebase = FirebaseAuth.DefaultInstance;
                Task<FirebaseUser> loginTask = firebase.SignInWithEmailAndPasswordAsync(email, pass);
                yield return loginTask.AsIEnumerator();
                FirebaseUser user;
                if (loginTask.IsFaulted)
                {
                    // user does not exist so we create it
                    Task<FirebaseUser> createUserTask = firebase.CreateUserWithEmailAndPasswordAsync(Constants.TEST_EMAIL, Constants.TEST_PASSWORD);
                    yield return createUserTask.AsIEnumerator();
                    try
                    {
                        user = createUserTask.Result;
                    }
                    catch (AggregateException)
                    {
                        // failed to create user
                        Debug.LogError(debugPrefix + "Failed to Create Test User: " + createUserTask.Exception.GetBaseException());
                        yield break;
                    }
                }
                user = loginTask.Result;
              

                if (!staySignedIn)
                {
                    Task nextAuthEvent = firebase.WaitForNextEvent("StateChanged");
                    firebase.SignOut();
                    yield return nextAuthEvent.AsIEnumerator();
                }
                Debug.Log(debugPrefix + "Finish");
                yield break;
            }
        }
    }

    namespace Util
    {
        public static class AssertAsync
        {
            public static async Task Throws<TException>(Func<Task> func)
            {
                var expected = typeof(TException);
                Type actual = null;
                try
                {
                    await func();
                }
                catch (Exception e)
                {
                    if (e is AggregateException)
                    {
                        e = e.GetBaseException();
                    }
                    actual = e.GetType();
                }
                Assert.AreEqual(expected, actual);
            }

        }

        public static class TaskExtensions
        {
            public static bool IsRunning(this Task<object> task) => !(task.IsCanceled || task.IsCompleted || task.IsFaulted);

            public static bool IsRunning(this Task task) => !(task.IsCanceled || task.IsCompleted || task.IsFaulted);
        }

        public static class Matcher
        {
            /// <summary>
            /// Checks to see if the arguments object is the same object o.
            /// Checks for reference equality
            /// </summary>
            /// <typeparam name="T">The argument type</typeparam>
            /// <param name="o">the object to check the argument reference with</param>
            /// <returns></returns>
            public static T Same<T>(T o)
            {
                return Moq.It.Is<T>(other => System.Object.ReferenceEquals(other, o));
            }
        }

        public static class EventHandlerExtensions
        {

            public static Task<T> WaitForNextEvent<T>(this object obj, string eventName)
            {
                var t = new TaskCompletionSource<T>();
                var evt = obj.GetType().GetEvent(eventName);
                EventHandler<T> x = null;
                x = (o, e) =>
                {
                    t.SetResult(e);
                    evt.RemoveEventHandler(obj, x);
                };
                evt.AddEventHandler(obj, x);
                return t.Task;
            }

            public static Task WaitForNextEvent(this object obj, string eventName)
            {
                var t = new TaskCompletionSource<EventArgs>();
                var evt = obj.GetType().GetEvent(eventName);
                EventHandler x = null;
                x = (o, e) =>
                {
                    t.SetResult(e);
                    evt.RemoveEventHandler(obj, x);
                };
                evt.AddEventHandler(obj, x);
                return t.Task;
            }
        }
    }

}