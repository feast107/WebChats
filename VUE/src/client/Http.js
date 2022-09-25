import requestInstance from "@/utils/request";
const AccessURL = "/Service/Access/"
export const signIn =(identity) =>
{
    return requestInstance({
        url: AccessURL+"SignIn",
        method:"post",
        data:identity
    })
}

export const current =() =>
{
    return requestInstance({
        url: AccessURL+"Current",
        method:"get",
    })
}

export const signOut = ()=>{
    return requestInstance({
        url: AccessURL+"SignOut",
        method:"get",
    })
}
