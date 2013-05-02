function Popup_init(inlay, show, hide) {
	inlay.clickOnEnter().hide();
	$(window).click(function (e) {
		var target = $(e.target);
		if (target.closest(show).length) Popup_show(inlay);
		if (target.closest(hide).length) Popup_hide(inlay);
	});
}

function Popup_show(inlay) {
	$('<div id="' + inlay[0].id + '_overlay" class="ui-widget-overlay" style="display:none">').insertAfter(inlay).fadeIn('fast');
	inlay.fadeIn('fast').center(true, true);
	var input = inlay.find('input:text, input:not([type])').first();
	if (!input.length) return;
	input = input.focus()[0];
	var length = input.value.length;
	if (document.selection) {
		var selection = input.createTextRange();
		selection.collapse(true);
		selection.moveEnd('character', length);
		selection.moveStart('character', length);
		selection.select();
	}
	else input.setSelectionRange(length, length);
}

function Popup_hide(inlay) {
	$('#' + inlay[0].id + '_overlay').fadeOut('fast', function () { $(this).remove() });
	inlay.fadeOut('fast').removeCenter();
}