function processResultsFromSingleSurvey(surveyBaseId) {
    const context = getContext();
    const container = context.getCollection();

    // Define the query to retrieve the survey by its ID
    const filterQuery = {
        query: 'SELECT * FROM item WHERE item.id = @surveyBaseId',
        parameters: [{ name: '@surveyBaseId', value: surveyBaseId }]
    };

    // Query the container for the survey
    const accept = container.queryDocuments(
        container.getSelfLink(),
        filterQuery,
        {},
        (err, feed) => {
            if (err) throw new Error(`Error querying survey: ${err.message}`);
            if (!feed || feed.length === 0)
                throw new Error(`Survey with id ${surveyBaseId} not found.`);

            // Log the title to verify it is being retrieved correctly
            console.log(feed[0].title);

            // Extract the survey and its responses
            const survey = feed[0];
            const title = survey.title || "";
            const responses = survey.responses || [];
            let results = [];

            // Process each response
            responses.forEach(response => {
                // Find or create a result object for the question
                let resultObject = results.find(r => r.questionId === response.questionId);
                if (!resultObject) {
                    resultObject = {
                        responseId: response.id,
                        title: response.questionTitle,
                        responseType: response.$responseType,
                        questionId: response.questionId,
                        results: []
                    };
                    results.push(resultObject);
                }

                // Process the response based on its type
                processResult(response, resultObject);
            });

            // Prepare the return object with the title and results
            const returnObject = {
                surveyTitle: title,
                results: results
            };

            // Set the return object as the response body
            context.getResponse().setBody(returnObject);
        }
    );

    if (!accept) {
        throw new Error("Stored procedure failed to query documents.");
    }

    // Helper function to process individual responses
    function processResult(responseObject, resultObject) {
        switch (responseObject.$responseType) {
            case "dateResponse":
                processDateResponse(responseObject, resultObject);
                break;
            case "textResponse":
                processTextResponse(responseObject, resultObject);
                break;
            case "ratingResponse":
                processRatingResponse(responseObject, resultObject);
                break;
            default:
                throw new Error(`Unhandled response type: ${responseObject.$responseType}`);
        }
    }

    // Handle dateResponse types
    function processDateResponse(responseObject, resultObject) {
        resultObject.results.push(responseObject.calendarDateResponse);
    }
    // Handle textResponse types
    function processTextResponse(responseObject, resultObject) {
        resultObject.results.push(responseObject.textQuestionResponse);
    }
    function processRatingResponse(responseObject, resultObject) {
        //array [choice, count of choices ]
        if (!responseObject.ratingResponse) {

        }
    }
}
