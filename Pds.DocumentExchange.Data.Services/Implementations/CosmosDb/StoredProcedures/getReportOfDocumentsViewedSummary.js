function getReportOfDocumentsViewedSummary(startTimeStr, endTimeStr) {
	var collection = getContext().getCollection();

	var startDate = getFormattedDate(startTimeStr);
	var endDate = getFormattedDate(endTimeStr);

    var isAccepted = collection.queryDocuments(
        collection.getSelfLink(),
        'SELECT ' +
            'Files.ToUKPRN, ' +
            'Files.FileType, ' +
            'Files.OriginalFileName ' +
        'FROM Batches ' +
        'JOIN Files in Batches.Files ' +
        'JOIN History in Files.History ' +
        'WHERE ' +
            'Files.FromUKPRN = -999 ' +
            'and Files.Okay = true ' +
            'and Files.Processed = true ' +
            'and History.Action = "ViewedByReciever" ' +
            'and History.ActionDateTimeUtc >= "' + startDate + '" ' +
            'and History.ActionDateTimeUtc <= "' + endDate + '"',
        {
            pageSize: -1
        },
        function (err, feed) {
            if (err) throw err;

            var fileGroups = new Map();

            for (var idx = 0, len = feed.length; idx < len; idx++) {
                var item = feed[idx];

                var key = item.ToUKPRN + "_" + item.FileType + "_" + item.OriginalFileName;

                if (!fileGroups.has(key)) {
                    fileGroups.set(key, {
                        ToUKPRN: item.ToUKPRN,
                        FileType: item.FileType,
                        OriginalFileName: item.OriginalFileName,
                        ClickCount: 0
                    });
                }

                fileGroups.get(key).ClickCount += 1;
            }

            var response = getContext().getResponse();
            response.setBody(Array.from(fileGroups.values()));
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