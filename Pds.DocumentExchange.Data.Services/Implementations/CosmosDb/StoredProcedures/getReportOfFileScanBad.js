function getReportFileScanBad(startTimeStr, endTimeStr) {
	var collection = getContext().getCollection();

	var startDate = getFormattedDate(startTimeStr);
	var endDate = getFormattedDate(endTimeStr);

    var isAccepted = collection.queryDocuments(
        collection.getSelfLink(),
        'SELECT ' +
            'Files.FromUKPRN, ' +
            'History.User, ' + 
            'Files.FileType, ' +
            'Files.OriginalFileName, ' +
            'History.ActionDateTimeUtc as ClickDateTimeUtc ' +
        'FROM Batches ' +
        'JOIN Files in Batches.Files ' +
        'JOIN History in Files.History ' +
        'WHERE ' +
            'Files.ToUKPRN = -999 ' +
            'and Files.Okay = true ' +
            'and Files.Processed = true ' +
            'and History.Action = "FileScanBad" ' +
            'and History.ActionDateTimeUtc >= "' + startDate + '" ' +
            'and History.ActionDateTimeUtc <= "' + endDate + '"',
        {
            pageSize: -1
        },
        function (err, feed) {
            if (err) throw err;

            var response = getContext().getResponse();
            response.setBody(feed);
        }
    );

    if (!isAccepted) {
        throw new Error('The query was not accepted by the server.');
    }

	function getFormattedDate(dateString) {
        var date = new Date(dateString);
		var year = date.getFullYear();

		var month = (1 + date.getMonth()).toString();
		month = month.length > 1 ? month : '0' + month;

		var day = date.getDate().toString();
		day = day.length > 1 ? day : '0' + day;
  
		return year + '-' + month + '-' + day;
	}
}