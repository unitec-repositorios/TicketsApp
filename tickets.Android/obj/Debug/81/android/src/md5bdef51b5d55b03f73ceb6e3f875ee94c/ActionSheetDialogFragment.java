package md5bdef51b5d55b03f73ceb6e3f875ee94c;


public class ActionSheetDialogFragment
	extends md5bdef51b5d55b03f73ceb6e3f875ee94c.AbstractBuilderDialogFragment_2
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_dismiss:()V:GetDismissHandler\n" +
			"";
		mono.android.Runtime.register ("Acr.UserDialogs.Fragments.ActionSheetDialogFragment, Acr.UserDialogs", ActionSheetDialogFragment.class, __md_methods);
	}


	public ActionSheetDialogFragment ()
	{
		super ();
		if (getClass () == ActionSheetDialogFragment.class)
			mono.android.TypeManager.Activate ("Acr.UserDialogs.Fragments.ActionSheetDialogFragment, Acr.UserDialogs", "", this, new java.lang.Object[] {  });
	}


	public void dismiss ()
	{
		n_dismiss ();
	}

	private native void n_dismiss ();

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
