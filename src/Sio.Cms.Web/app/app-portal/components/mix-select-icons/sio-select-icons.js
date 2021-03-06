
modules.component('sioSelectIcons', {
    templateUrl: '/app/app-portal/components/sio-select-icons/sio-select-icons.html',
    controller: ['$rootScope', '$scope', '$location', function ($rootScope, $scope, $location) {
        var ctrl = this;
        ctrl.translate = function (keyword) {
            return $rootScope.translate(keyword);
        };
        ctrl.select = function(ico){
            ctrl.data = ico;
        }
    }],
    bindings: {
        data: '=',
        prefix: '=',
        options: '=',
    }
});
