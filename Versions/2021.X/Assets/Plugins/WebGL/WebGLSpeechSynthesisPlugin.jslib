mergeInto(LibraryManager.library, {
	WebGLSpeechSynthesisPluginIsAvailable: function () {
		if (typeof speechSynthesis === "undefined") {
			return false;
		} else {
			return true;
		}
	},
	WebGLSpeechSynthesisPluginGetVoices: function () {
		var returnStr = "";
		if (typeof speechSynthesis === "undefined") {
			// not supported
		} else {
			var voices = document.mWebGLSpeechSynthesisPluginVoices;
			if (voices != undefined) {
				var jsonData = {};
				jsonData.voices = [];
				for (var voiceIndex = 0; voiceIndex < voices.length; ++voiceIndex) {
					var voice = voices[voiceIndex];
					var speechSynthesisVoice = {};
					speechSynthesisVoice._default = voice.default; //default is reserved word
					speechSynthesisVoice.lang = voice.lang;
					speechSynthesisVoice.localService = voice.localService;
					speechSynthesisVoice.name = voice.name;
					speechSynthesisVoice.voiceURI = voice.voiceURI;
					jsonData.voices.push(speechSynthesisVoice);
				}
				//console.log(JSON.stringify(jsonData, null, 2));
				returnStr = JSON.stringify(jsonData);
			}
		}
		var bufferLength = lengthBytesUTF8(returnStr) + 1;
		var buffer = _malloc(bufferLength);
		if (stringToUTF8 == undefined) {
			writeStringToMemory(returnStr, buffer);
		} else {
			stringToUTF8(returnStr, buffer, bufferLength);
		}
		return buffer;
	},
	WebGLSpeechSynthesisPluginInit: function () {
		//console.log("WebGLSpeechSynthesisPlugin: Init");
		if (typeof speechSynthesis === "undefined") {
			return;
		}
		if (document.mWebGLSpeechSynthesisPluginVoices != undefined) {
			return; //already initialized
		}
		var initVoices = function () {
			var voices = speechSynthesis.getVoices();
			if (voices.length == 0) {
				setTimeout(function () { initVoices() }, 10);
				return;
			}
			document.mWebGLSpeechSynthesisPluginVoices = voices;
			//console.log(document.mWebGLSpeechSynthesisPluginVoices);
		}
		initVoices();
	},
	WebGLSpeechSynthesisPluginCreateSpeechSynthesisUtterance: function () {
		if (typeof speechSynthesis === "undefined") {
			return -1;
		}
		if (document.mWebGLSpeechSynthesisPluginUtterances == undefined) {
			document.mWebGLSpeechSynthesisPluginUtterances = [];
		}
		var index = document.mWebGLSpeechSynthesisPluginUtterances.length;
		var instance = new SpeechSynthesisUtterance();
		document.mWebGLSpeechSynthesisPluginUtterances.push(instance);
		return index;
	},
	WebGLSpeechSynthesisPluginSetUtterancePitch: function (index, pitch) {
		if (typeof speechSynthesis === "undefined") {
			return;
		}
		if (document.mWebGLSpeechSynthesisPluginUtterances == undefined) {
			return;
		}
		if (document.mWebGLSpeechSynthesisPluginUtterances.length <= index) {
			return;
		}
		var instance = document.mWebGLSpeechSynthesisPluginUtterances[index];
		if (instance == undefined) {
			return;
		}
		var strPitch = UTF8ToString(pitch);
		instance.pitch = parseFloat(strPitch);
	},
	WebGLSpeechSynthesisPluginSetUtteranceRate: function (index, rate) {
		if (typeof speechSynthesis === "undefined") {
			return;
		}
		if (document.mWebGLSpeechSynthesisPluginUtterances == undefined) {
			return;
		}
		if (document.mWebGLSpeechSynthesisPluginUtterances.length <= index) {
			return;
		}
		var instance = document.mWebGLSpeechSynthesisPluginUtterances[index];
		if (instance == undefined) {
			return;
		}
		var strRate = UTF8ToString(rate);
		instance.rate = parseFloat(strRate);
	},
	WebGLSpeechSynthesisPluginSetUtteranceText: function (index, text) {
		if (document.mWebGLSpeechSynthesisPluginUtterances == undefined) {
			console.error('WebGLSpeechSynthesisPluginSetUtteranceText: Utterances are undefined!');
			return;
		}
		if (document.mWebGLSpeechSynthesisPluginUtterances.length <= index) {
			console.error('WebGLSpeechSynthesisPluginSetUtteranceText: Index out of bounds!');
			return;
		}
		var instance = document.mWebGLSpeechSynthesisPluginUtterances[index];
		if (instance == undefined) {
			console.error('WebGLSpeechSynthesisPluginSetUtteranceText: Instance is undefined!');
			return;
		}
		instance.text = UTF8ToString(text);
		//console.log('WebGLSpeechSynthesisPluginSetUtteranceText: ', instance.text);
	},
	WebGLSpeechSynthesisPluginSetUtteranceVoice: function (index, voiceURI) {
		if (document.mWebGLSpeechSynthesisPluginUtterances == undefined) {
			return;
		}
		if (document.mWebGLSpeechSynthesisPluginUtterances.length <= index) {
			return;
		}
		var instance = document.mWebGLSpeechSynthesisPluginUtterances[index];
		if (instance == undefined) {
			return;
		}
		var voices = document.mWebGLSpeechSynthesisPluginVoices;
		if (voices == undefined) {
			return;
		}
		var strVoice = UTF8ToString(voiceURI);
		//console.log("SetUtteranceVoice: " + voiceURI);
		for (var voiceIndex = 0; voiceIndex < voices.length; ++voiceIndex) {
			var voice = voices[voiceIndex];
			if (voice == undefined) {
				continue;
			}
			if (voice.voiceURI == strVoice) {
				//console.log("SetUtteranceVoice: Matched voice " + voice.name);
				instance.voice = voice;
				return;
			}
		}
		console.error("SetUtteranceVoice: Failed to match voice!", index, voiceURI);
	},
	WebGLSpeechSynthesisPluginSetUtteranceVolume: function (index, volume) {
		if (document.mWebGLSpeechSynthesisPluginUtterances == undefined) {
			return;
		}
		if (document.mWebGLSpeechSynthesisPluginUtterances.length <= index) {
			return;
		}
		var instance = document.mWebGLSpeechSynthesisPluginUtterances[index];
		if (instance == undefined) {
			return;
		}
		instance.volume = volume;
	},
	WebGLSpeechSynthesisPluginSpeak: function (index) {
		if (typeof speechSynthesis === "undefined") {
			console.error('WebGLSpeechSynthesisPluginSpeak: speechSynthesis type is undefined!');
			return;
		}
		if (document.mWebGLSpeechSynthesisPluginUtterances == undefined) {
			console.error('WebGLSpeechSynthesisPluginSpeak: utterances are undefined!');
			return;
		}
		if (document.mWebGLSpeechSynthesisPluginUtterances.length <= index) {
			console.error('WebGLSpeechSynthesisPluginSpeak: index is out of range!');
			return;
		}
		var instance = document.mWebGLSpeechSynthesisPluginUtterances[index];
		if (instance == undefined) {
			console.error('WebGLSpeechSynthesisPluginSpeak: voice not set!');
			return;
		}
		//console.log('WebGLSpeechSynthesisPluginSpeak: setting callback for synthesis onend');
		instance.onend = function (event) {
			//console.log('WebGLSpeechSynthesisPluginSpeak: synthesis onend callback invoked');
			var jsonData = {};
			jsonData.index = index;
			jsonData.elapsedTime = event.elapsedTime;
			jsonData.type = event.type;
			//console.log(JSON.stringify(jsonData, null, 2));
			if (document.mWebGLSpeechSynthesisPluginOnEnd == undefined) {
				document.mWebGLSpeechSynthesisPluginOnEnd = [];
			}
			document.mWebGLSpeechSynthesisPluginOnEnd.push(JSON.stringify(jsonData));
		}
		//console.log('WebGLSpeechSynthesisPluginSpeak: invoke speak', instance.text);
		speechSynthesis.speak(instance);
		//console.log('WebGLSpeechSynthesisPluginSpeak: speak invoked', instance.text);
	},
	WebGLSpeechSynthesisPluginHasOnEnd: function () {
		if (document.mWebGLSpeechSynthesisPluginOnEnd == undefined) {
			document.mWebGLSpeechSynthesisPluginOnEnd = [];
		}
		return (document.mWebGLSpeechSynthesisPluginOnEnd.length > 0);
	},
	WebGLSpeechSynthesisPluginGetOnEnd: function () {
		var returnStr = "";
		if (document.mWebGLSpeechSynthesisPluginOnEnd == undefined) {
			document.mWebGLSpeechSynthesisPluginOnEnd = [];
		}
		if (document.mWebGLSpeechSynthesisPluginOnEnd.length == 0) {
			returnStr = "No results available";
		} else {
			returnStr = document.mWebGLSpeechSynthesisPluginOnEnd[0];
		}
		document.mWebGLSpeechSynthesisPluginOnEnd = document.mWebGLSpeechSynthesisPluginOnEnd.splice(1);
		var bufferLength = lengthBytesUTF8(returnStr) + 1;
		var buffer = _malloc(bufferLength);
		if (stringToUTF8 == undefined) {
			writeStringToMemory(returnStr, buffer);
		} else {
			stringToUTF8(returnStr, buffer, bufferLength);
		}
		return buffer;
	},
	WebGLSpeechSynthesisPluginCancel: function () {
		if (typeof speechSynthesis === "undefined") {
			return;
		}
		speechSynthesis.cancel();
	}
});
