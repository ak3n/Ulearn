import React from 'react';
import ReactDOM from 'react-dom';
import UlearnApp from './App';
import "./externalComponentRenderer";

import 'moment/locale/ru';
import "moment-timezone";

const root = document.getElementById('root');

if (root) {
	ReactDOM.render((
		<UlearnApp />
	), root);
}



/* TODO (andgein):
* Replace with
*
* import { unregister } from './registerServiceWorker';
* unregister()
*
* in future. */
function unregisterServiceWorker() {
	if (window.navigator && navigator.serviceWorker) {
		navigator.serviceWorker.getRegistrations()
		.then(function (registrations) {
			for (let registration of registrations) {
				registration.unregister();
			}
		});
	}
}

unregisterServiceWorker();