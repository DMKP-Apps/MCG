var $services = {};

function dataViewModelProperty(parent, key, value)
{
    var self = this;
    self.parent = parent;
    self.key = key;
    self.value = value;
}

function dataViewModel(parent, options, onUpdate, onUpdateDone, onPreUpdate) {
    var self = this;
    self.parent = parent;
    self.properties = [];
    for (var key in options) {
        var value = options[key];
        if (value != null && value.toString().indexOf("/Date(") > -1) {
            options[key] = moment(value).toDate();
        }

        var prop = new dataViewModelProperty(self, key, options[key]);
        self.properties.push(prop);
    }

    self.data = ko.observable(ko.mapping.fromJS(options));
    self.onPreUpdate = onPreUpdate == null ? function () { } : onPreUpdate;
    self.onUpdate = onUpdate == null ? function (data) { return { done: function (data) { return { complete: function (data) { } } } }; } : onUpdate;
    self.onUpdateDone = onUpdateDone == null ? function (item, data) { self.parent.isLoading(false); } : onUpdateDone;


    self.submit = function () {
        var data = ko.mapping.toJS(self.data());
        self.onPreUpdate();
        self.parent.isLoading(true);
        self.onUpdate(data).done(function (data) {
            self.parent.success("Update has been successfully completed!");
            self.onUpdateDone(self, data);
        })
        .error(function (data) {
            self.parent.error("An error occurred during update.");
            self.onUpdateDone(self, data);
        })
        .complete(function () {

        });
    };

};