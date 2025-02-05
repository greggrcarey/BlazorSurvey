function processResultsFromSingleSurvey(surveyBaseId) {
    const context = getContext();
    const container = context.getCollection();

    // Initialize the results array
    let results = [];

    // Define the query to retrieve the survey by its ID
    const filterQuery = {
        query: 'SELECT * FROM item WHERE item.id = @surveyBaseId',
        parameters: [{ name: '@surveyBaseId', value: surveyBaseId }]
    };

    // Query the container for the survey
    const accept = container.queryDocuments(container.getSelfLink(), filterQuery, {}, (err, feed) => {
        if (err) throw new Error(`Error querying survey: ${err.message}`);
        if (!feed || feed.length === 0) throw new Error(`Survey with id ${surveyBaseId} not found.`);

        // Extract responses from the survey document
        const responses = feed[0].responses || [];

        // Process each response
        responses.forEach(response => {
            // Check if a resultObject already exists for this questionId
            let resultObject = results.find(r => r.questionId === response.questionId);

            if (!resultObject) {
                // If no resultObject exists, create a new one
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
    });

    if (!accept) {
        throw new Error("Stored procedure failed to query documents.");
    }

    // Set the results array as the response body
    context.getResponse().setBody(results);

    // Helper function to process individual responses
    function processResult(responseObject, resultObject) {
        switch (responseObject.$responseType) {
            case "dateResponse":
                processDateResponse(responseObject, resultObject);
                break;
            default:
                throw new Error(`Unhandled response type: ${responseObject.$responseType}`);
        }
    }

    // Handle dateResponse types
    function processDateResponse(responseObject, resultObject) {
        // Append the calendarDateResponse to the results array of the resultObject
        resultObject.results.push(responseObject.calendarDateResponse);
    }
}
