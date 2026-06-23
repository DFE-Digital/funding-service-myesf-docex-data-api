function getCurrentVersionNumber(fromUkprn, toUkprn, year, fileType) {
    var collection = getContext().getCollection();

    var isAccepted = collection.queryDocuments(
        collection.getSelfLink(),
	'SELECT ' +
        'value count(1) ' +
    'FROM Batches ' +
    'JOIN Files in Batches.Files ' +
    'WHERE ' +
        'Files.ToUKPRN = ' + parseInt(toUkprn) + ' ' +
        'and Files.FromUKPRN = ' + parseInt(fromUkprn) + ' ' +
        'and Files.Metadata.Year = "' + parseInt(year) + '" ' +
        'and Files.FileType = "' + parseInt(fileType) + '"',

    function (err, feed) {
        if (err) throw err;

        var response = getContext().getResponse();

        // Check the feed and if empty, set the body to 'no docs found', 
        // else take 1st element from feed
        if (!feed || !feed.length) {            
            response.setBody(0);
        }
        else {
            response.setBody(parseInt(feed[0]));
        }
    });

    if (!isAccepted) throw new Error('The query was not accepted by the server.');
}