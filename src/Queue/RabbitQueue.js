(function (global, factory) {
    if (typeof define === "function" && define.amd) {
        define(["exports", "babel-runtime/core-js/json/stringify", "babel-runtime/helpers/classCallCheck", "amqplib/callback_api"], factory);
    } else if (typeof exports !== "undefined") {
        factory(exports, require("babel-runtime/core-js/json/stringify"), require("babel-runtime/helpers/classCallCheck"), require("amqplib/callback_api"));
    } else {
        var mod = {
            exports: {}
        };
        factory(mod.exports, global.stringify, global.classCallCheck, global.callback_api);
        global.unknown = mod.exports;
    }
})(this, function (exports, _stringify, _classCallCheck2, _callback_api) {
    "use strict";

    Object.defineProperty(exports, "__esModule", {
        value: true
    });
    exports.subscribeToQueue = exports.publishMessageToQueue = exports.QueueSettings = exports.Message1 = undefined;

    var _stringify2 = _interopRequireDefault(_stringify);

    var _classCallCheck3 = _interopRequireDefault(_classCallCheck2);

    var $import0 = _interopRequireWildcard(_callback_api);

    function _interopRequireWildcard(obj) {
        if (obj && obj.__esModule) {
            return obj;
        } else {
            var newObj = {};

            if (obj != null) {
                for (var key in obj) {
                    if (Object.prototype.hasOwnProperty.call(obj, key)) newObj[key] = obj[key];
                }
            }

            newObj.default = obj;
            return newObj;
        }
    }

    function _interopRequireDefault(obj) {
        return obj && obj.__esModule ? obj : {
            default: obj
        };
    }

    var Message1 = exports.Message1 = function Message1($arg0) {
        (0, _classCallCheck3.default)(this, Message1);
        this.Body = $arg0;
    };

    var QueueSettings = exports.QueueSettings = function QueueSettings($arg0, $arg1, $arg2, $arg3) {
        (0, _classCallCheck3.default)(this, QueueSettings);
        this.UserName = $arg0;
        this.Password = $arg1;
        this.HostName = $arg2;
        this.ExchangeName = $arg3;
    };

    var channelClosed = function (connection, error) {
        connection.close();
        null;
    };

    var channelCreated = function (settings, queueName, message, connection, error, channel) {
        var messageBuffer = function (args) {
            return new Buffer(args);
        }((0, _stringify2.default)(message));

        var options = {
            durable: true
        };
        channel.assertQueue(queueName, options);
        channel.sendToQueue(queueName, messageBuffer);
        channel.close(function (delegateArg0) {
            channelClosed(connection, delegateArg0);
        });
    };

    var connected = function (a_0, a_1, a_2, err, connection) {
        var a = [a_0, a_1, a_2];
        connection.createChannel(function (delegateArg0, delegateArg1) {
            var settings = a[0];
            var queueName = a[1];
            var message = a[2];
            channelCreated(settings, queueName, message, connection, delegateArg0, delegateArg1);
        });
    };

    var publishMessageToQueue = exports.publishMessageToQueue = function (settings, message, queueName) {
        $import0.connect("amqp://" + settings.HostName, function (delegateArg0, delegateArg1) {
            connected(settings, queueName, message, delegateArg0, delegateArg1);
        });
    };

    var subscribeToQueue = exports.subscribeToQueue = function (queueName, settings, handler) {
        null;
    };
});
//# sourceMappingURL=RabbitQueue.js.map