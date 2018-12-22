import React from 'react';
import ReactDOM from 'react-dom';
import UlearnApp from './App';
import registerServiceWorker from './registerServiceWorker';
import { DEFAULT_TIMEZONE } from './consts/general'

import moment from "moment";
import 'moment/locale/ru';
import "moment-timezone";

moment.tz.setDefault(DEFAULT_TIMEZONE);

ReactDOM.render((
    <UlearnApp />
), document.getElementById('root'));

registerServiceWorker();
