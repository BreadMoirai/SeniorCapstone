﻿using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using UnityEngine;
using Object = System.Object;
using RestSharp;
using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;
using Firebase.Storage;
using System.Threading.Tasks;
using Firebase.Auth;
using UnityEngine.Networking;
using System.Text;
using UnityEngine.UI;
/// <summary>
/// Manages recipe reviews
///
/// Ruben Sanchez
/// </summary>
public class ReviewController : MonoBehaviour
{

    #region Properties

    public Recipe recipe
    {
        set => recipeNameText.text = value.Name;
    }

    [SerializeField] private StarController stars;
    [SerializeField] private InputField inputField;
    [SerializeField] private Text recipeNameText;

    #endregion
    //Database endpoint
    public static string Endpoint = "https://regen-66cf8.firebaseio.com/";

    //Here is the backend handling most database calls

    public static DatabaseManager Instance;

    //firebase object
    private DatabaseReference databaseReference;
    private bool hasAttemptFinished;
    public List<Review> reviewList = new List<Review>();
    #region Public Methods

    public void DidTapSubmit()
    {
        RatingReviewing review = new RatingReviewing(stars.GetNumberOfActiveStars(), inputField.text, RecipeManagerUI.currentRecipe);
        gameObject.SetActive(false);
        // TODO: Send review to backend service for publishing
    }

    public void Reset()
    {
        inputField.text = "";
        stars.Reset();
    }

    #endregion
    public void getReviews(string recipeID)
    {
        hasAttemptFinished = false;
        reviewList.Clear();
        StartCoroutine(WaitForReviews());
        FirebaseDatabase.DefaultInstance
               .GetReference("reviews").Child(recipeID)
               .GetValueAsync().ContinueWith(task =>
               {
                   if (task.IsFaulted)
                   {
                       print("faulted");
                       hasAttemptFinished = true;

                   }
                   else if (task.IsCompleted)
                   {
                       if (task.Result.ChildrenCount == 0)
                           return;

                       DataSnapshot snapshot = task.Result;

                       foreach (var s in snapshot.Children)
                       {
                           Debug.Log(s.GetRawJsonValue());
                           Review newReview = JsonUtility.FromJson<Review>(s.GetRawJsonValue());
                           newReview.userId = s.Key;
                           Console.WriteLine(newReview.content);
                           reviewList.Add(newReview);
                       }

                       hasAttemptFinished = true;
                   }
               });
    }
    private IEnumerator WaitForReviews()
    {
        yield return new WaitUntil(() => hasAttemptFinished);
        foreach (Review r in reviewList)
        {
            Debug.Log(r.userId);
        }

    }
}

struct RatingReviewing
{
    string review;
    Recipe recipe;
    int stars;
    public RatingReviewing(int stars, string review, Recipe recipe)
    {
        this.review = review;
        this.stars = stars;
        this.recipe = recipe;
    }
}



