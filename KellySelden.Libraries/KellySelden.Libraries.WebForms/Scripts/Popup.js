function Popup_init(clientId, options, openTriggerId, closeTriggerId) {
	var popup = $('#' + clientId).dialog(options);
	if (openTriggerId) {
		$('#' + openTriggerId).click(function() {
			popup.dialog('open');
			return false;
		});
	}
	if (closeTriggerId) {
		$('#' + closeTriggerId).click(function() {
			popup.dialog('close');
			return false;
		});
	}
}
function Popup_open(clientId) {
	$('#' + clientId).dialog('open');
}