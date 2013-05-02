function Player(id, type, youtube) {
	var div = $('#' + id);
	if (type == 'YouTube')
		$('<iframe frameborder="0" allowfullscreen></iframe>')
			.attr('src', 'http://www.youtube.com/embed/' + youtube + '?wmode=opaque&rel=0')
			.attr('height', div.height())
			.attr('width', div.width())
			.appendTo(div);
	else {
		var video = type == 'Video';
		div.flowplayer({
			src: '/flowplayer/flowplayer-3.2.7.swf',
			wmode: 'opaque'
		}, {
			plugins: {
				controls: {
					fullscreen: video,
					autoHide: video

//				backgroundColor: "transparent",
//				backgroundGradient: "none",
//				sliderColor: '#FFFFFF',
//				sliderBorder: '1.5px solid rgba(160,160,160,0.7)',
//				volumeSliderColor: '#FFFFFF',
//				volumeBorder: '1.5px solid rgba(160,160,160,0.7)',

//				timeColor: '#ffffff',
//				durationColor: '#535353',

//				tooltipColor: 'rgba(255, 255, 255, 0.7)',
//				tooltipTextColor: '#000000'
				}
			},
			clip: {
				autoPlay: video
			}
		});
	}
}