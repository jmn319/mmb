function HelloController($scope) 
{ $scope.greeting = { text: 'Hello #!~' }; }

var album = [{name:'Southwest Serenade', duration: '2:34'},
{name:'Northern Light Waltz', duration: '3:21'},
{name:'Eastern Tango', duration: '17:45'}];

function AlbumController($scope) 
{ $scope.album = album; }

var ModalDemoCtrl = function ($scope) {
  $scope.template = { url: 'modal.html'};
  $scope.add = function () {
        $scope.$broadcast('showModal');
    };
  
  $scope.$on('showModal', function($scope)
  {
	angular.element("#_Modal").modal("show"); 
  });
  
  $scope.open = function () {
    $scope.shouldBeOpen = true;
	$scope.greeting = { text: 'Hi' };
  };

  $scope.close = function () {
    $scope.closeMsg = 'I was closed at: ' + new Date();
    $scope.shouldBeOpen = false;
  };

  $scope.items = ['item1', 'item2'];

  $scope.opts = {
    backdropFade: true,
    dialogFade:true
  };

};
