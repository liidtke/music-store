import dayjs from 'dayjs';
import LocalizedFormat from 'dayjs/plugin/localizedFormat.js';
import customParseFormat from 'dayjs/plugin/customParseFormat.js';
import pt from 'dayjs/locale/pt-br.js';
import utc from 'dayjs/plugin/utc.js';
import timezone from 'dayjs/plugin/timezone.js';

export class DateConverter {

    private format: string = 'L';
    locale: 'pt';
    tzone:'America/Sao_Paulo'
    
    constructor() {
        dayjs.extend(LocalizedFormat);
        dayjs.extend(customParseFormat);
        dayjs.extend(utc);
        dayjs.extend(timezone);
        dayjs.tz.setDefault(this.tzone);
        dayjs.locale(pt);
    }

  get dayjs(){
        return dayjs;
  }

    toString(value, customFormat = null){
        //console.log('converting value', value)
        if(!value){
            return ''
        }

        if(typeof(value) === 'string'){
            //console.log('value is string')
        }

        let date = dayjs(value).utc().local();
        //console.log(date, value)
        let str = date.format(customFormat || this.format);
        //console.log(str);
        return str;
    }

    parse(value:string){
        //console.log('parsing value', value)
        let newDate = dayjs.utc(value, this.format).local();
        if(newDate.isValid()){
            return newDate.toISOString().replace('Z', '');
        }
        else return false
    }


// Format the date to "YYYY-MM-DD"
    toInput(input) {
        if(!input){return ''};
        let date = new Date(input);
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0'); // Months are 0-indexed
        const day = String(date.getDate()).padStart(2, '0');

        return  `${year}-${month}-${day}`;
    }

  toInputHour(input) {

    console.log('start ', input)
    let date = dayjs()
    if(input){
      date = dayjs(input);
    }
  
    let r0 =  date.format('YYYY-MM-DDTHH:mm');
    return r0;
  }
    
    now(){
        return dayjs().toISOString();
    }
}

const dateConverter = new DateConverter();

export default dateConverter;
