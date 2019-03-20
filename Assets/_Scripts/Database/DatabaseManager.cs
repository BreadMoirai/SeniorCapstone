﻿using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using UnityEngine;
using Object = System.Object;
using RestSharp;
using System;
using Newtonsoft.Json;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance;
    
    private DatabaseReference databaseReference;

    private bool hasAttemptFinished;

    private List<Recipe> currentRecipes = new List<Recipe>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        // Set up the Editor before calling into the realtime database.
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://regen-66cf8.firebaseio.com/");
        // Get the root databaseReference location of the database.
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

        TestPublish("Hawaiian Pizza");
        TestPublish("Hawaiian Rolls");
        TestPublish("Hawaiian Salmon");


        //Search("Hawaiian");
    }

    private void PublishNewRecipe(Recipe recipe)
    {
        string key = databaseReference.Child("recipes").Push().Key;

        string recipeNameTrimmed = recipe.Name.Trim();
        recipeNameTrimmed = recipeNameTrimmed.Replace(" ", "");

        recipe.ImageReferencePath = $"gs://regen-66cf8.appspot.com/Recipes/{recipeNameTrimmed}{key}.jpg";

        string json = JsonUtility.ToJson(recipe);

        databaseReference.Child("recipes").Child(key).SetRawJsonValueAsync(json);
    }
    public void elasticSearch(string name)
    {
        var client = new RestClient("http://35.192.138.105/elasticsearch/_search/template");
        var request = new RestRequest(Method.GET);
        request.AddHeader("Postman-Token", "33d32a58-494a-49fb-8bb8-924d330ad906");
        request.AddHeader("cache-control", "no-cache");
        request.AddHeader("Authorization", "Basic dXNlcjpYNE1keTVXeGFrbVY=");
        request.AddHeader("Content-Type", "application/json");
        request.AddParameter("application/json","{ \"query\":{ \"bool\":{ \"must\":[],\"must_not\":[{\"match_all\":{}}],\"should\":[{\"match_all\":{}}]}},\"from\":0,\"size\":10,\"sort\":[],\"aggs\":{}}", ParameterType.RequestBody);
        IRestResponse response = client.Execute(request);
        print(response.Content);
    }
    public void elasticSearchExclude(string name,string[] excludeTags)
    {
        var client = new RestClient("http://35.192.138.105/elasticsearch/_search/template");
        var request = new RestRequest(Method.GET);

        string param = "\"{\"source\": {\"query\": {\"bool\": {";
        string must_not = "\"must_not\":[";
        string Excludetag = "{\"term\":{\"tags\":\"";
        string should = "\"should\":[{\"wildcard\":{\"name\":{\"value\":" + name + "\"}},{\"fuzzy\":{\"name\":{\"value\":\"" + name + "\"}}}]}}";
        string size = "\"size\" : 10";
        request.AddHeader("Postman-Token", "f1918e1d-0cbd-4373-b9e6-353291796dd6");
        request.AddHeader("cache-control", "no-cache");
        request.AddHeader("Authorization", "Basic dXNlcjpYNE1keTVXeGFrbVY=");
        request.AddHeader("Content-Type", "application/json");
        if (excludeTags.Length > 0)
        {
            param = param + must_not;
            for(int i=0; i < excludeTags.Length; i++)
            {
                if(i !=0) {
                    param += ",";
                }
                param = param + Excludetag + excludeTags[i] + "\"}}";
            }
            param += "],";
            param = param + should + size;
            
        }
        else
        {
            param = "{\"source\": { \"query\": {\"bool\": {\"should\": [ {\"wildcard\": " +
                "{\"{{my_field1}}\": \"*{{my_value}}*\"}},{\"fuzzy\": {\"{{my_field1}}\": \"{{my_value}}\"}}, {\"wildcard\": " +
                "{\"{{my_field2}}\": \"*{{my_value}}*\"}},{\"fuzzy\": {\"{{my_field2}}\": \"{{my_value}}\"}},{\"wildcard\": {\"{{my_field3}}\": " +
                "\"*{{my_value}}*\"}},{\"fuzzy\": {\"{{my_field3}}\": \"{{my_value}}\"}}]}},\"size\": \"{{my_size}}\"},\"params\": {\"my_field1\": " +
                "\"name\",\"my_field2\": \"ingredients\",\"my_field3\": \"tags\",\"my_value\": \"" + name +
                "\",\"my_size\": 100}}";
        }

        request.AddParameter("application/json", "{\n    \"source\": {\n        \"query\": {\n            \"bool\": {\n                \"must_not\": [\n                    {\n                        \"fuzzy\": {\n                            \"{{my_field3}}\": \"{{my_tags}}\"\n                        }\n                    },\n                    {\n                        \"wildcard\": {\n                            \"{{my_field3}}\": \"*{{my_tags}}*\"\n                        }\n                    }\n                ],\n                \"should\":[\n                \t                    {\n                        \"wildcard\": {\n                            \"{{my_field1}}\": \"*{{my_value}}*\"\n                        }\n                    },\n                    {\n                        \"fuzzy\": {\n                            \"{{my_field1}}\": \"{{my_value}}\"\n                        }\n                    }\n                \t]\n            }\n        },\n        \"size\": \"{{my_size}}\"\n    },\n    \"params\": {\n        \"my_field1\": \"name\",\n        \"my_field3\": \"tags\",\n        \"my_value\": \"endies\",\n        \"my_tags\": \"asdfa\",\n        \"my_size\": 100\n    }\n}", ParameterType.RequestBody);
        IRestResponse response = client.Execute(request);
        print(response.Content);
    }
    public void Search(string name)
    {
        hasAttemptFinished = false;
        currentRecipes.Clear();

        StartCoroutine(WaitForRecipes());

        FirebaseDatabase.DefaultInstance
            .GetReference("recipes").OrderByChild("Name")
            .StartAt(name)
            .EndAt(name + "\uf8ff")
            .GetValueAsync().ContinueWith(task => {
                if (task.IsFaulted)
                {
                    // Handle the error...
                }
                else if (task.IsCompleted)
                {
                    if (task.Result.ChildrenCount == 0)
                        return;

                    DataSnapshot snapshot = task.Result;
                    print(snapshot.GetRawJsonValue());

                    foreach (var recipe in snapshot.Children)
                    {
                        Recipe newRecipe = JsonUtility.FromJson<Recipe>(recipe.GetRawJsonValue());
                        currentRecipes.Add(newRecipe);
                    }
                }

                hasAttemptFinished = true;
            });
    }

    private IEnumerator WaitForRecipes ()
    {
        yield return new WaitUntil(() => hasAttemptFinished);

        // if search has yielded results, update the recipe list in the ui
        if(currentRecipes.Count > 0)
            SearchManagerUI.Instance.RefreshRecipeList(currentRecipes);
    }

    private void TestPublish(string name)
    {
        List<Ingredient> ingredients = new List<Ingredient>()
        {
            new Ingredient("flour", "1/2 cup"),
            new Ingredient("marinara", "1/2 cup"),
            new Ingredient("mozzerella", "2 cups"),
            new Ingredient("ham", "1/3 cup"),
            new Ingredient("pineapple", "1/4 cup")
        };

        List<string> steps = new List<string>()
        {
            "Knead the dough.",
            "Add the marinara sauce.",
            "Add the mozerrella cheese.",
            "Add the ham.",
            "Add the pineapple.",
            "Bake at 360F for 45 minutes."
        };

        List<string> tags = new List<string>()
        {
            "dairy"
        };

        List<string> reviews = new List<string>()
        {
            "This was pretty ok."
        };

        Recipe newRecipe = new Recipe(name, "", 450, 50, tags, ingredients, steps, reviews, 4);

        PublishNewRecipe(newRecipe);
    }
}
