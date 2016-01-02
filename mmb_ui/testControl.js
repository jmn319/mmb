//var myApp = angular.module('myApp', []);

function testControl($scope) 
{
    $scope.view = { name: '' };
	$scope.myshows = [{ Name: "You have no shows in your queue..." }];
	$scope.pendingshows = [{ Name: "There are no pending shows..." }];
	$scope.downloadedshows = [{ Name: "There are no recent downloaded shows..." }];
	$scope.mymovies = [{ Name: "You have no movies in your queue..." }];
	$scope.pendingmovies = [{ Name: "There are no pending movies..." }];
	$scope.downloadedmovies = [{ Name: "There are no recent downloaded movies..." }];
	$scope.show_index = 0;
	$scope.movie_index = 0;
	$scope.addmovie = { Name: '' };
	$scope.searchIMDBMovie = "";
	$scope.searchLocalMovie = "";
	$scope.selectedMovie = "";
	$scope.searchShow = "";
	$scope.selectedShow = "";
	$scope.searchResults = [];
	$scope.imdbMovieSearchList = false;
	$scope.localMovieSearchList = false;
	$scope.showSearchList = false;
	
	//[Route("/get/{MediaType}/{All}/{Pending}/{Downloaded}", "GET")]
	//Get my shows
	$.getJSON("http://newyork-1226.com:26263/get/show/false/false/false", null, function(data)
		{ if (data.length === 0); else $scope.myshows = data; });
	//Get pending shows
	$.getJSON("http://newyork-1226.com:26263/get/show/false/true/false", null, function(data)
		{ if (data.length === 0); else $scope.pendingshows = data; });
	//Get downloaded shows
	$.getJSON("http://newyork-1226.com:26263/get/show/false/false/true", null, function(data)
		{ if (data.length === 0); else $scope.downloadedshows = data; });
	//Get my movies
	$.getJSON("http://newyork-1226.com:26263/get/movie/false/false/false", null, function(data)
		{ if (data.length === 0); else $scope.mymovies = data; });
	//Get pending movies
	$.getJSON("http://newyork-1226.com:26263/get/movie/false/true/false", null, function(data)
		{ if (data.length === 0); else $scope.pendingmovies = data; });
	//Get downloaded movies
	$.getJSON("http://newyork-1226.com:26263/get/movie/false/false/true", null, function(data)
		{ if (data.length === 0); else $scope.downloadedmovies = data; });
		
	//Deprecated large pulls of all movies & shows
	
	$scope.setView = function(path)
	{ 
		$scope.view.name = path; 
		if (path == "show_table.html")
			$.getJSON("http://newyork-1226.com:26263/get/show/false/false/false", null, function(data)
				{ if (data.length === 0); else $scope.myshows = data; });
		else if (path == "pending_show_table.html")
			$.getJSON("http://newyork-1226.com:26263/get/show/false/true/false", null, function(data)
				{ if (data.length === 0); else $scope.pendingshows = data; });	
		else if (path == "downloaded_show_table.html")
			$.getJSON("http://newyork-1226.com:26263/get/show/false/false/true", null, function(data)
				{ if (data.length === 0); else $scope.downloadedshows = data; });
		else if (path == "movie_table.html")
			$.getJSON("http://newyork-1226.com:26263/get/movie/false/false/false", null, function(data)
				{ if (data.length === 0); else $scope.mymovies = data; });
		else if (path == "pending_movie_table.html")
			$.getJSON("http://newyork-1226.com:26263/get/movie/false/true/false", null, function(data)
				{ if (data.length === 0); else $scope.pendingmovies = data; });
		else if (path == "downloaded_movie_table.html")
			$.getJSON("http://newyork-1226.com:26263/get/movie/false/false/true", null, function(data)
				{ if (data.length === 0); else $scope.downloadedmovies = data; });
	}
	
	$scope.clearView = function()
	{ $scope.view.name = ''; }
	
	$scope.displayShowModifyModal = function(show, modalName)
	{ 
		$scope.show_index = $scope.myshows.indexOf(show); 
		$('#' + modalName).modal('show'); 
	}
	
	$scope.displayMovieModifyModal = function(m, modalName)
	{ 
		$scope.movie_index = $scope.mymovies.indexOf(m); 
		$('#' + modalName).modal('show'); 
	}
	
	$scope.displayModal = function(modalName)
	{ $('#' + modalName).modal('show'); }
	
	$scope.saveModifyInfo = function()
	{ $.post("http://newyork-1226.com:26263/shows/myshows/update", $scope.myshows[$scope.show_index]); }
	
	$scope.searchShows = function(searchText)
	{ 
		$.getJSON("http://newyork-1226.com:26263/search/show/true/" + searchText, null, function(data)
			{ $scope.$apply(function(){$scope.searchResults = data}); });
		$scope.showSearchList = true; 
	}
	
	$scope.saveAddShow = function(selected)
	{ 
		$scope.searchResults[$scope.searchShowIndex(selected.Name)].ModType = "add";
		$scope.searchResults[$scope.searchShowIndex(selected.Name)].MediaType = "show";
		$scope.searchResults[$scope.searchShowIndex(selected.Name)].Season = selected.Season;
		$scope.searchResults[$scope.searchShowIndex(selected.Name)].Episode = selected.Episode;
		$.post("http://newyork-1226.com:26263/modify", $scope.searchResults[$scope.searchShowIndex(selected.Name)], function() {
			$.getJSON("http://newyork-1226.com:26263/get/show/false/false/false", null, function(data)
				{ if (data.length === 0); else $scope.$apply(function(){ $scope.myshows = data; });});
		});
		$scope.showSearchList = false;
	}
	
	$scope.saveModifyShow = function(selected)
	{ 
		selected.ModType = "modify";
		selected.MediaType = "show";
		$.post("http://newyork-1226.com:26263/modify", selected, function() {
			$.getJSON("http://newyork-1226.com:26263/get/show/false/false/false", null, function(data)
				{ if (data.length === 0); else $scope.$apply(function(){ $scope.myshows = data; });});
		});
	}
	
	$scope.deleteShow = function(selected)
	{ 
		$scope.myshows[$scope.myShowIndex(selected)].ModType = "delete";
		$scope.myshows[$scope.myShowIndex(selected)].MediaType = "show";
		$.post("http://newyork-1226.com:26263/modify", $scope.myshows[$scope.myShowIndex(selected)], function() {
			$.getJSON("http://newyork-1226.com:26263/get/show/false/false/false", null, function(data)
				{ if (data.length === 0); else $scope.$apply(function(){ $scope.myshows = data; });});
		});
	}
	
	$scope.searchIMDB = function(searchText)
	{ 
		$.getJSON("http://newyork-1226.com:26263/search/movie/false/" + searchText, null, function(data)
			{ $scope.$apply(function(){$scope.searchResults = data}); });
		$scope.localMovieSearchList = false;
		$scope.imdbMovieSearchList = true; //add back to false when save movie or close modal
	}
	
	$scope.searchMovieLocal = function(searchText)
	{ 
		$.getJSON("http://newyork-1226.com:26263/search/movie/true/" + searchText, null, function(data)
			{ $scope.$apply(function(){$scope.searchResults = data}); });
		$scope.imdbMovieSearchList = false;
		$scope.localMovieSearchList = true; //add back to false when save movie or close modal
	}
	
	$scope.saveMovie = function(selected)
	{
		$scope.searchResults[$scope.searchIndex(selected)].ModType = "add";
		$scope.searchResults[$scope.searchIndex(selected)].MediaType = "movie";
		$.post("http://newyork-1226.com:26263/modify", $scope.searchResults[$scope.searchIndex(selected)], function() {
			$.getJSON("http://newyork-1226.com:26263/get/movie/false/false/false", null, function(data)
				{ if (data.length === 0); else $scope.$apply(function(){ $scope.mymovies = data; });});
		});
		//parse result - if success then add to table
		$scope.imdbMovieSearchList = false;
		$scope.localMovieSearchList = false;
	}
	
	$scope.deleteMovie = function(selected)
	{
		var currentMovie = $scope.mymovies[$scope.myMovieIndex(selected)];
		currentMovie.ModType = "delete";
		currentMovie.MediaType = "movie";
		$.post("http://newyork-1226.com:26263/modify", currentMovie, function() {
			$.getJSON("http://newyork-1226.com:26263/get/movie/false/false/false", null, function(data)
				{ if (data.length === 0); else $scope.$apply(function(){ $scope.mymovies = data; });});
		});
	}
	
	$scope.dismissSearchModal = function()
	{
		$scope.selectedMovie = '';
		$scope.searchMovie = '';
		$scope.selectedShow = '';
		$scope.$apply(function(){$scope.searchShow = ""});
		$scope.imdbMovieSearchList = false;
		$scope.localMovieSearchList = false;
		$scope.showSearchList = false;
		$scope.$apply(function(){$scope.searchResults = []});
	}
	
	$scope.searchIndex = function(selected)
	{
		var addM = selected.split(" (")[0];
		for (i = 0; i < $scope.searchResults.length; i++)
		{
			if (addM === $scope.searchResults[i].Name)
				return i;
		}
	}
	
	$scope.searchShowIndex = function(selected)
	{
		for (i = 0; i < $scope.searchResults.length; i++)
		{
			if (selected === $scope.searchResults[i].Name)
				return i;
		}
	}
	
	$scope.myMovieIndex = function(selected)
	{
		for (i = 0; i < $scope.mymovies.length; i++)
		{
			if (selected === $scope.mymovies[i].Name)
				return i;
		}
	}
	
	$scope.myShowIndex = function(selected)
	{
		for (i = 0; i < $scope.myshows.length; i++)
		{
			if (selected === $scope.myshows[i].Name)
				return i;
		}
	}
}