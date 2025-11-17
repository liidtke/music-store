export class Result {

  constructor(isSuccess:boolean, headers:Headers){
    this.success = isSuccess;
    this.message = "";
    this.isAdmin = false;
    this.role = "Anon";
    if(headers){
      this.setHeaders(headers);
    }
  }

  private success:boolean;
  message:string;
  data:any;
  isAdmin:boolean;
  role:string;
  subscription:string;

  
  get isSuccess(){
    return this.success;
  }

  get isError(){
    return !this.success;
  }

  setHeaders(headers:Headers){
    if(headers.has('x-us-role')){
      this.role = headers.get('x-us-role')
      this.isAdmin = this.role == "AdminRole";
    }

    this.subscription = headers.get("x-us-subscription");
  }

  static async ParseResponse(response:Response){
    if(response.ok){
      var result = new Result(true, response.headers);
      result.data = await response.json(); 
      return result;
    }
    else{
      var result = new Result(false, null);
      
      console.log(response);
      let message = "Erro"
      try{

        message = await response.text();

      }
      catch(err){

      }
      result.message = message;
      return result;
    }
  }

  static ParseError(e){
    console.warn(e);
    return new Result(false);
  }

}

  export interface IFieldValidation{
  message:string;
  field:string;
}
